using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace LiveStreamBridge
{
    public class CustomButtonData : IExposable
    {
        public string name = "新按钮";
        public List<string> incidentDefNames = new List<string>();

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Collections.Look(ref incidentDefNames, "incidentDefNames", LookMode.Value);
        }

        public void Execute(Map map, int times, string sender)
        {
            if (incidentDefNames.NullOrEmpty()) return;
            times = Mathf.Clamp(times, 1, 10);

            for (int i = 0; i < times; i++)
            {
                string defName = incidentDefNames.RandomElement();
                IncidentDef def = DefDatabase<IncidentDef>.GetNamedSilentFail(defName);
                if (def == null)
                {
                    Messages.Message(defName + " 事件不存在，已跳过。", MessageTypeDefOf.NeutralEvent);
                    continue;
                }

                IncidentParms parms = StorytellerUtility.DefaultParmsNow(def.category, map);
                try
                {
                    if (def.Worker.TryExecute(parms))
                    {
                        Messages.Message(sender + " 触发了 " + def.LabelCap + "!",
                            MessageTypeDefOf.PositiveEvent);
                    }
                    else
                    {
                        // 事件尝试执行但返回失败（通常条件不满足）
                        Messages.Message(def.LabelCap + " 触发失败（条件不满足）。",
                            MessageTypeDefOf.NeutralEvent);
                    }
                }
                catch (Exception e)
                {
                    // 友好提示，替换红色报错
                    string reason = "需要特定条件";
                    if (def.defName == "CaravanDemand")
                        reason = "当前地图没有商队";
                    else if (def.defName == "RansomDemand")
                        reason = "当前地图没有被绑架的殖民者";
                    else if (def.defName == "GiveQuest_Beggars" || def.defName == "GiveQuest_ReliquaryPilgrims")
                        reason = "该事件不适合在此地图触发";

                    Messages.Message(def.LabelCap + " 无法触发：" + reason,
                        MessageTypeDefOf.NeutralEvent);
                    Log.Warning("LiveStreamBridge: " + def.defName + " 失败: " + e.Message);
                }
            }
        }
    }

    public class LiveStreamBridgeSettings : ModSettings
    {
        public List<CustomButtonData> customButtons = new List<CustomButtonData>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref customButtons, "customButtons", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && customButtons.NullOrEmpty())
                InitDefaultButtons();
        }

        public void InitDefaultButtons()
        {
            customButtons = new List<CustomButtonData>();

            var threats = new List<string>();
            var rewards = new List<string>();
            foreach (var def in GetAvailableIncidents())
            {
                if (def.category == IncidentCategoryDefOf.ThreatBig)
                    threats.Add(def.defName);
                else
                    rewards.Add(def.defName);
            }

            if (threats.Count > 0)
                customButtons.Add(new CustomButtonData { name = "⚠️ 随机威胁", incidentDefNames = threats });
            if (rewards.Count > 0)
                customButtons.Add(new CustomButtonData { name = "✅ 随机奖励", incidentDefNames = rewards });
        }

        private static List<IncidentDef> GetAvailableIncidents()
        {
            var list = new List<IncidentDef>();
            foreach (IncidentDef def in DefDatabase<IncidentDef>.AllDefs)
            {
                if (def.workerClass != null && def.category != null &&
                    def != IncidentDefOf.RaidFriendly)
                    list.Add(def);
            }
            return list;
        }
    }

    public class LiveStreamBridgeMod : Mod
    {
        public static LiveStreamBridgeSettings settings;

        public LiveStreamBridgeMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<LiveStreamBridgeSettings>();
        }

        public override string SettingsCategory() { return "直播互动按钮"; }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            list.Label("请在游戏内按钮面板中管理事件。");
            list.End();
        }
    }

    public class MainButtonWorker_Gifts : MainButtonWorker
    {
        public override void Activate() { Find.WindowStack.Add(new Window_GiftPanel()); }
    }

    public class Window_GiftPanel : Window
    {
        private float countFloat = 1f;
        private int countInt = 1;
        private string senderName = "观众";

        public override Vector2 InitialSize { get { return new Vector2(360f, 400f); } }

        public Window_GiftPanel()
        {
            draggable = true;
            doCloseButton = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("🎲 自定义事件触发");

            listing.Label("数量: " + ((int)countFloat).ToString());
            Rect sliderRect = listing.GetRect(24f);
            countFloat = GUI.HorizontalSlider(sliderRect, countFloat, 1f, 10f);
            countFloat = (float)Mathf.RoundToInt(countFloat);
            countInt = (int)countFloat;

            listing.Gap(4f);

            listing.Label("触发者:");
            senderName = listing.TextEntry(senderName);

            listing.Gap();
            listing.Label("────────────────");

            var buttons = LiveStreamBridgeMod.settings.customButtons;
            foreach (var btn in buttons)
            {
                if (listing.ButtonText(btn.name))
                    Trigger(btn, countInt);
            }

            listing.Gap();
            listing.Label("────────────────");

            if (listing.ButtonText("⚙️ 管理按钮"))
                Find.WindowStack.Add(new Window_ManageButtons());

            listing.End();
        }

        private void Trigger(CustomButtonData btn, int count)
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                Messages.Message("没有可用地图（请先进入一个地图）", MessageTypeDefOf.RejectInput);
                return;
            }
            btn.Execute(map, count, senderName);
        }
    }

    public class Window_ManageButtons : Window
    {
        private Vector2 scrollPos;
        private string newName = "";

        public override Vector2 InitialSize { get { return new Vector2(500f, 600f); } }

        public Window_ManageButtons()
        {
            draggable = true;
            doCloseButton = true;
            forcePause = true;
        }

        public override void Close(bool doCloseSound = true)
        {
            LiveStreamBridgeMod.settings.Write();
            base.Close(doCloseSound);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            var buttons = LiveStreamBridgeMod.settings.customButtons;
            Rect scrollRect = list.GetRect(400f);
            float height = buttons.Count * 90f + 200f;

            Widgets.BeginScrollView(scrollRect, ref scrollPos,
                new Rect(0, 0, scrollRect.width - 16f, height));

            float y = 0;
            for (int i = 0; i < buttons.Count; i++)
            {
                var btn = buttons[i];
                Rect row = new Rect(0, y, scrollRect.width - 16f, 80f);
                btn.name = Widgets.TextField(new Rect(row.x, row.y, 200, 30), btn.name);

                if (Widgets.ButtonText(new Rect(210, row.y, 120, 30), "选择事件"))
                    Find.WindowStack.Add(new Window_SelectIncidents(btn));

                if (Widgets.ButtonText(new Rect(340, row.y, 120, 30), "删除"))
                {
                    buttons.RemoveAt(i);
                    break;
                }
                y += 90f;
            }

            Widgets.EndScrollView();
            list.Gap();

            list.Label("新按钮:");
            newName = list.TextEntry(newName);
            if (list.ButtonText("添加") && !string.IsNullOrEmpty(newName))
            {
                buttons.Add(new CustomButtonData { name = newName });
                newName = "";
            }

            list.End();
        }
    }

    public class Window_SelectIncidents : Window
    {
        private CustomButtonData target;
        private Vector2 scroll;
        private List<IncidentDef> availableDefs;
        private HashSet<string> selected;
        private string searchText = "";

        public override Vector2 InitialSize { get { return new Vector2(400f, 500f); } }

        public Window_SelectIncidents(CustomButtonData btn)
        {
            target = btn;
            draggable = true;
            doCloseButton = true;
            forcePause = true;

            availableDefs = new List<IncidentDef>();
            foreach (IncidentDef def in DefDatabase<IncidentDef>.AllDefs)
            {
                if (def.workerClass != null && def.category != null &&
                    def != IncidentDefOf.RaidFriendly)
                    availableDefs.Add(def);
            }
            selected = new HashSet<string>(btn.incidentDefNames);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            list.Label("选择事件 - " + target.name);

            list.Label("搜索:");
            string newSearch = list.TextEntry(searchText);
            if (newSearch != searchText)
            {
                searchText = newSearch;
            }

            List<IncidentDef> filteredDefs = new List<IncidentDef>();
            if (string.IsNullOrEmpty(searchText))
            {
                filteredDefs = availableDefs;
            }
            else
            {
                string lowerSearch = searchText.ToLower();
                foreach (IncidentDef def in availableDefs)
                {
                    if (def.label != null && def.label.ToLower().Contains(lowerSearch))
                        filteredDefs.Add(def);
                }
            }

            list.Gap();

            Rect scrollRect = list.GetRect(300f);
            float contentHeight = filteredDefs.Count * 25f;
            if (contentHeight < 100f) contentHeight = 100f;

            Widgets.BeginScrollView(scrollRect, ref scroll,
                new Rect(0, 0, scrollRect.width - 16f, contentHeight));

            for (int i = 0; i < filteredDefs.Count; i++)
            {
                IncidentDef def = filteredDefs[i];
                bool has = selected.Contains(def.defName);
                Rect rowRect = new Rect(0, i * 25f, scrollRect.width - 16f, 24f);
                Widgets.CheckboxLabeled(rowRect, def.label, ref has);
                if (has)
                    selected.Add(def.defName);
                else
                    selected.Remove(def.defName);
            }

            Widgets.EndScrollView();

            if (list.ButtonText("确定"))
            {
                target.incidentDefNames = new List<string>(selected);
                Close();
            }
            list.End();
        }
    }
}