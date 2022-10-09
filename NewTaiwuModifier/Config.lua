return {
	DefaultSettings = 
	{
		[1] = 
		{
			Options = 
			{
				[1] = "随机",
				[2] = "早熟",
				[3] = "均衡",
				[4] = "晚成"
			},
			Description = "可以指定太吾的技艺资质成长类型",
			DisplayName = "技艺资质成长类型",
			SettingType = "Dropdown",
			Key = "lifeSkillGrowthType",
			DefaultValue = 1
		},
		[2] = 
		{
			Options = 
			{
				[1] = "随机",
				[2] = "早熟",
				[3] = "均衡",
				[4] = "晚成"
			},
			Description = "可以指定太吾的武学资质成长类型",
			DisplayName = "武学资质成长类型",
			SettingType = "Dropdown",
			Key = "combatSkillGrowthType",
			DefaultValue = 1
		},
		[3] = 
		{
			MaxValue = 145,
			MinValue = 0,
			StepSize = 5,
			Description = "太吾的平均技艺资质(无加成)不会低于设置",
			DisplayName = "技艺资质最小均值",
			SettingType = "Slider",
			Key = "minLifeSkillAvg",
			DefaultValue = 60
		},
		[4] = 
		{
			MaxValue = 145,
			MinValue = 0,
			StepSize = 5,
			Description = "太吾的平均武学资质(无加成)不会低于设置",
			DisplayName = "武学资质最小均值",
			SettingType = "Slider",
			Key = "minCombatSkillAvg",
			DefaultValue = 60
		},
		[5] = 
		{
			Description = "开局特质免费且不再提示有未使用的点数",
			DisplayName = "免费特质",
			SettingType = "Toggle",
			Key = "freeTrait",
			DefaultValue = false
		},
		[6] =
		{
			Description = "资质设定对谷中密友同步生效",
			DisplayName = "谷密同步资质",
			SettingType = "Toggle",
			Key = "closeFriendSync",
			DefaultValue = false
		},
		[7] = 
		{
			MinValue = 0,
			StepSize = 1,
			MaxValue = 10,
			DisplayName = "最少正面特性数",
			SettingType = "Slider",
			Key = "minGoodBasicFeatures",
			DefaultValue = 0
		},
		[8] = 
		{
			MinValue = 0,
			StepSize = 1,
			MaxValue = 10,
			DisplayName = "最大负面特性数",
			SettingType = "Slider",
			Key = "maxBadBasicFeatures",
			DefaultValue = 10
		},
		[9] = 
		{
			MinValue = 0,
			StepSize = 1,
			MaxValue = 10,
			DisplayName = "最少特性数",
			SettingType = "Slider",
			Key = "minBasicFeatures",
			DefaultValue = 0
		}
	},
	FileId = 2871619538,
	Author = "EveningTwilight",
	Description = [[自定义开局 V1.0.3
新开局时，可自定义资质成长类型
资质最小均值
正负面特性数量(对谷中密友同步生效)
开局特质免费
PS:	第一次上传的时候，带上了旧的Settings.lua，如果出现设置排版问题，可以尝试删除{你的Steam目录}\steamapps\workshop\content\838350\2871619538\Settings.lua

更新日志:
	v1.0.3 修复'早熟'与'均衡'颠倒，支持资质和成长类型同步谷密
	2022.10.6 v1.0.2 支持开局特质免费，并且不提示有未使用的点数
	2022.10.6 v1.0.1 修复NPC特性消失的问题(把true/false写反了，建了三个存档都没发现_(:з」∠)_]],
	Source = 1,
	HasArchive = false,
	Title = "自定义开局",
	Cover = "Cover.png",
	FrontendPlugins = 
	{
		[1] = "NewTaiwuModifier_Front.dll"
	},
	BackendPlugins = 
	{
		[1] = "NewTaiwuModifier.dll"
	}
}