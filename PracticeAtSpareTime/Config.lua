return {
	DefaultSettings = 
	{
		[1] = 
		{
			MinValue = 0,
			MaxValue = 100,
			Description = [[每个太吾忙碌而没有修炼的日子进行练功的概率
由于太吾十分繁忙，只会随机挑一门功法进行修炼]],
			DisplayName = "修习概率",
			SettingType = "Slider",
			Key = "combatSkillPracticeRate",
			DefaultValue = 50
		},
		[2] =
		{
			MinValue = 0,
			MaxValue = 10,
			Description = [[单次过月每次成功练功后，下次成功概率递减值]],
			DisplayName = "修习概率递减",
			SettingType = "Slider",
			Key = "combatSkillProbDec",
			DefaultValue = 2
		},
		[3] = 
		{
			Description = "开启过月自动修习会消耗历练",
			DisplayName = "消耗历练",
			SettingType = "Toggle",
			Key = "combatSkillCostExp",
			DefaultValue = false
		},
		[4] = 
		{
			Description = "开启后会打印日志，没遇到问题的话不用打开",
			DisplayName = "debug",
			SettingType = "Toggle",
			Key = "debug",
			DefaultValue = false
		},
	},
	FileId = 2871620206,
	Author = "EveningTwilight",
	Description = [[闲时练功 v1.1.0
勤勉的太吾，即使忙碌了一天，也会挤出时间来练功
启用mod后，根据每个月太吾没有修习功法的天数
过月时会随机修习功法
TODO: 优化选择修习功法的逻辑

更新日志:
	v1.1.0 增加历练消耗选项、成功率递减
			单次过月每次成功自动练功后，当月下一次练功的基础成功率递减
			(例如50%基础成功率，递减值为2，有两天roll点，两天都成功的概率为50%*48%，小削一下)]],
	Source = 1,
	HasArchive = false,
	Title = "闲时练功",
	Cover = "Cover.png",
	BackendPlugins = 
	{
		[1] = "PracticeAtSpareTime.dll"
	}
}