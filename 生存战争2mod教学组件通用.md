# 生存战争2 #
## mod组件介绍 ##
**本文档由咲夜留乃制作请勿用作商业用途，后果自负！**
> 组件信息由“**小宁game**”提供相关链接：[视频教程]("B站")
## 生物信息 ##
Creature 组件

1. Description 动物(人物)在游戏中的简介

2. Category，生物类别

3. DisplayName，动物(人物)在游戏中的名字。

4. ConstantSpawn，恒定出生

5. KillVerbs，死亡信息中，显示被某物杀死的名称
## Body身体组件 ##

1. BoxSize游戏实体碰撞箱大小。

2. Mass 实体重量在游戏中实体重量
## 音效组件 ##
CreatureSounds 音效组件

>组件下有三个声音分别是，被某物攻击声音，攻击某物声音，无行为声音

1. PainSound，实体被人物或者是其它生物攻击所发生声音。

2. AttackSound，实体攻击人物，或者是攻击其它生物发生出的声音。

3. IdleSound、实体在平常，在没有任何行为下所发出的声音注:在部分生物也就是没有攻击性的生物，就不会有AttackSound

4.  ldleSoundMinDistance、动物发出声音最小声音距离，目前仅在鬣狗、野牛、火鸡、鸵鸟、乌鸦、虎、麻雀等出现
## 掉落组件 ##
Loot 掉落组件

> 掉落组件格式
> `<ParameterSetName="Loot"Guid="00a4f5b5-f0be-4887-9583">Name="1"Guid="5cd5df7f-1df9-6b47"Value="DiamondChunkBlock;1;1;0.05"Type="string" />`

加入分号可以决定掉落数量，比如1;10，每次掉落1~10个，再加入分号可决定掉落几率。如;0.50当然想要同时掉落2个物品以上，复制Loot组件，把组件中“Name”中序号改一下即可
## 凝视行为及狼相关 ##
StareBehavior凝视行为组件

1. StareRange凝视距离，可以理解动物对你发起攻击最大距离，参数越大
动物对你发起攻击(格数)距离越远，只在攻击性动物出现该组件。 

2. GlowingEyesColor狼变成狼人眼睛显示的颜色(RGB)。

3. GlowingEyesOffsetr狼人发光眼睛位置偏移(眼睛位置)
 HowlBehavior狼的吼叫行为该组件只在狼，狼人、鬣狗出现

4. HowlSoundName，动物吼叫音频文件路径(对应资源库Assets\Auido).
## 血量组件 ##
Health 血量组件

1. AttackResilience 这个参数决定了这个生物(人物)最大生命值。

2. FallResilience 掉落抗性，该参数越大生物(人物)对于掉落伤害越小，反之参数越小掉落伤害越高，
参数决定在游戏中相对应的方块格数。

3. FireResilience，修改该参数后生物(人物)提升防火抗性，该参数越大，生物(人物)受到“火焰”
伤害越小。
## 特殊组件 ##
HumanModel组件

目前只在玩家和狼人中出现

1. WalkLegsAngle狼人(人物)腿的动画角度，以弧度为单位，修改该参数后参数越大，腿的弧度角度
越大

3. TextureOverride纹理材质顺序

4. WalkAnimationSpeed步行动画速度，修改该参数后，生物(人物)腿在行走时，参数越大运动角度
越小。



> 带动物相关 Model词汇的是模型组件模板(如FourLeggedModel)，它决定了动物能用
的模型种类(例如你不能在使用FourLeggedModel组件的实体上使用某条鱼的模型)
## 攻击组件 ##
Miner 攻击组件

1. AttackPower攻击组件，玩家和动物每次的攻击伤害，参数越大攻击伤害越高，伤害根据参数数值决
## 追逐行为 ##
ChaseBehavior追逐行为

1. ChaseNonPlayerProbability追逐非玩家的生物概率，这里的“非玩家”指的是攻击性动物攻击其它
动物而不是攻击玩家的概率，例如狼攻击猪和牛等。

2. ChaseWhenAttackedProbability攻击性动物攻击动物的概率，如狼攻击猪和牛。

3. NightChaseTime攻击性动物在夜间攻击动物，每次发起攻击时间，以秒为单位

4. DayChaseRange攻击性动物在白天，攻击动物或玩家，每次攻击发起该范围

5. ChaseOnTouchProbability追逐点触概率，玩家点了动物，然后命中后判定次点触概率,如果成功就
会追逐玩家

6. DayChaseTime攻击性动物在白天攻击动物，每次发起攻击时间，以秒为单位

7. NightChaseRange攻击性动物在夜间，攻击动物或玩家，每次攻击发起该范围

8. AutoChaseMask要追逐的生物类别
## 运动组件 ##
Locomotion运动组件

1. WalkSpeedWhenTurning生物奔跑时的速度，为0时奔跑无法旋转，只能向前走

2. LadderSpeed 生物以及人物上楼梯速度

3. InAirWalkFactor生物和人物空气行走的摩擦系数。

4. WalkSpeed 生物陆地奔跑速度(寻常走路无效,攻击、逃跑等生效)。

5. FlySpeed飞行，生物(人物)飞行速度(M/S)默认情况下为0

6. TurnSpeed 生物旋转方向时的速度

7. JumpSpeed 生物跳跃速度(与生物奔跑速度计算跳跃高度)
## 变形组件 ##
Shapeshifter变形组件

1. DayEntityTemplateName在白天变形对应实体模板的名称，字符串为空则不变形

2. NightEntityTemplateName在夜间变形对应实体模板的名称，字符串为空则不变形。
## 蛋组件 ##
CreatureEggData

1. TextureSlot 纹理材质槽，决定该生物蛋颜色、

2. Color游戏中生物蛋颜色，RGB决定该生物蛋的颜色

3. DisplayName在游戏中显示该生物蛋的名字

4. EggTypelndex生物序号，需要按照ID 顺序填写

5. ShowEgg 是否能在游戏中查看该生物蛋
## 模型组件 ##
HumanModel

1. ModelName 实体模型路径

2. TextureOverride实体贴图路径

3. SwimAnimationSpeed 鱼类在水中速度