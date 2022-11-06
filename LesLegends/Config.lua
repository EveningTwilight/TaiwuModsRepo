return {
	DefaultSettings = 
	{
		[1] = 
		{
			Description = "谷中密友与太吾同性别且为双性恋",
			DisplayName = "谷中姬友",
			SettingType = "Toggle",
			Key = "closeFriendFixBisexual",
			DefaultValue = false
		},
		[2] =
		{
			Description = "隐秘小村NPC与太吾同性别的必为双性恋",
			DisplayName = "隐村姬友",
			SettingType = "Toggle",
			Key = "presetNPCFixBisexual",
			DefaultValue = false
		},
		[3] =
		{
			Description = "同性欢爱不会破身(不影响怀孕，感谢茄子，这俩特性不同组)",
			DisplayName = "不破神功",
			SettingType = "Toggle",
			Key = "keepVirginity",
			DefaultValue = false
		},
		[4] =
		{
			Description = "新生儿性别和性取向概率的设置全局生效",
			DisplayName = "新生设置全局生效",
			SettingType = "Toggle",
			Key = "forceSexualRateForAll",
			DefaultValue = false
		},
		[5] = 
		{
			Options = 
			{
				[1] = "随机",
				[2] = "太吾",
				[3] = "太吾对象"
			},
			Description = "太吾与同性发生关系时，指定怀孕的一方",
			DisplayName = "怀孕指定",
			SettingType = "Dropdown",
			Key = "fixMotherType",
			DefaultValue = 1
		},
		[6] = 
		{
			Options = 
			{
				[1] = "默认",
				[2] = "不生蛐蛐",
				[3] = "只生蛐蛐"
			},
			Description = "修改太吾的后代是否为蛐蛐的策略",
			DisplayName = "太吾蛐蛐",
			SettingType = "Dropdown",
			Key = "fixCricketLuckType",
			DefaultValue = 1
		},
		[7] = 
		{
			MaxValue = 100,
			MinValue = 0,
			StepSize = 1,
			Description = [[春宵命中率的最小值，如5则至少有5%概率怀孕，仅对同性生效]],
			DisplayName = "春宵最低命中率",
			SettingType = "Slider",
			Key = "minSexHitProb",
			DefaultValue = 0
		},
		[8] =
		{
			MaxValue = 100,
			MinValue = 0,
			StepSize = 1,
			Description = [[春宵命中率的最大值，如50则至多有50%概率怀孕，仅对同性生效]],
			DisplayName = "春宵最高命中率",
			SettingType = "Slider",
			Key = "maxSexHitProb",
			DefaultValue = 100
		},
		[9] = 
		{
			Description = "父母同性别时，子女性别固定为该性别",
			DisplayName = "遗传性别",
			SettingType = "Toggle",
			Key = "sameSexualFromParents",
			DefaultValue = false
		},
		[10] = 
		{
			Description = [[父母都为双性恋或者父母性别相同，则子女必定双性恋
未启用时，父母性别相同或为双性恋仍会增加子女双性恋概率]],
			DisplayName = "遗传性向",
			SettingType = "Toggle",
			Key = "bisexualFromParents",
			DefaultValue = false
		},
		[11] = 
		{
			MaxValue = 100,
			MinValue = 0,
			StepSize = 1,
			Description = [[调整新生儿出生为女性的概率(默认50)
(Tips:0的话全是男童)]],
			DisplayName = "新生女性概率",
			SettingType = "Slider",
			Key = "newBornsFemaleRate",
			DefaultValue = 50
		},
		[12] = 
		{
			MaxValue = 100,
			MinValue = 0,
			StepSize = 1,
			Description = [[调整新生儿出生为双性恋的概率(默认20)
(本mod涉及的双性恋概率基于这一项为基准)
(想要杜绝的话，可以改config文件拖到-100)]],
			DisplayName = "新生双向概率",
			SettingType = "Slider",
			Key = "newBornsBisexualRate",
			DefaultValue = 20
		},
		[13] = {
			MaxValue = 24,
			MinValue = 14,
			StepSize = 1,
			Description = "倾诉爱意最小年龄(默认16)",
			DisplayName = "倾诉爱意最小年龄",
			SettingType = "Slider",
			Key = "minMarrayAge",
			DefaultValue = 16
		},
		[14] = {
			Description = [[比武招亲检查性取向而非单纯性别(实际结婚时按照真实性别)
仅修改了入口事件和终点事件的判定
由于比武招亲事件较多，修改还未验证
可能会出现结婚对象错误之类的情况，发现后再修复]],
			DisplayName = "比武招亲",
			SettingType = "Toggle",
			Key = "bisexualMarray",
			DefaultValue = false
		},
		[15] = {
			Description = "共结连理及比武招亲无视已婚状态",
			DisplayName = "允许重婚",
			SettingType = "Toggle",
			Key = "unlimitMarray",
			DefaultValue = false
		},
		[16] = {
			Description = "倾诉爱意和共结连理无视门派限制",
			DisplayName = "门派解限",
			SettingType = "Toggle",
			Key = "unlimitMonk",
			DefaultValue = false
		},
		[17] = {
			Description = "倾诉爱意和共结连理无视血缘关系限制",
			DisplayName = "亲缘解限",
			SettingType = "Toggle",
			Key = "unlimitBlood",
			DefaultValue = false
		},
		[18] = {
			Description = "倾诉爱意无视师徒关系",
			DisplayName = "师徒解限",
			SettingType = "Toggle",
			Key = "unlimitMentor",
			DefaultValue = false
		},
		[19] = {
			Description = "共结连理无视太吾对目标好感",
			DisplayName = "无视好感",
			SettingType = "Toggle",
			Key = "unlimitFavor",
			DefaultValue = false
		},
		[20] = {
			Description = "对话事件解限对异性恋生效(真有用这mod的太吾玩异性恋?)",
			DisplayName = "对话事件修改异性恋",
			SettingType = "Toggle",
			Key = "modifyForDifSexual",
			DefaultValue = false
		},
		[21] = 
		{
			Description = "开启后会打印大量日志，没遇到问题的话就关掉",
			DisplayName = "debug",
			SettingType = "Toggle",
			Key = "debug",
			DefaultValue = false
		}
	},
	FileId = 2871617896,
	Author = "EveningTwilight",
	Description = [[姬友传说 v1.1.3
支持指定谷中密友和隐秘小村NPC的性向
支持设定新生儿男女比例、双性恋概率、同性生殖(及成功率)、是否生蛐蛐等
说是姬友传说，其实也可以是基佬传说
免责声明:
本mod仅为娱乐之用，不代表作者对各种性向的态度
也不带任何宣传教唆之意，未成年人请在家长陪同下使用

更新日志:
	v1.1.3 完善全局性别及取向概率指定(仅余留门派限定性别的部分阶层不修改性别)，性取向概率不仅影响人物取向，他们的配偶生成时，性别会符合他们的取向
	v1.1.2 远走高飞事件支持解除限制、不破神功仅对太吾生效(以后视情况支持设置);修复游戏中更改设置无效的问题、去掉官方已经修复的怀孕锁保护
	v1.1.1 修复会导致其他mod失效的问题、修复桂州比武招亲入口修改只对刚正妹子生效的问题
	更早版本记录见Mod目录下Updates.txt]],
	Source = 1,
	HasArchive = false,
	Title = "姬友传说",
	Cover = "Cover.png",
	BackendPlugins = 
	{
		[1] = "LesLegends.dll",
		[2] = "LesLegends_Event.dll"
	}
}