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
		[5] = 
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
		[6] = 
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
		[7] =
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
		[8] = 
		{
			Description = "父母同性别时，子女性别固定为该性别",
			DisplayName = "遗传性别",
			SettingType = "Toggle",
			Key = "sameSexualFromParents",
			DefaultValue = false
		},
		[9] = 
		{
			Description = [[父母都为双性恋或者父母性别相同，则子女必定双性恋
未启用时，父母性别相同或为双性恋仍会增加子女双性恋概率]],
			DisplayName = "遗传性向",
			SettingType = "Toggle",
			Key = "bisexualFromParents",
			DefaultValue = false
		},
		[10] = 
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
		[11] = 
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
		[13] = 
		{
			Description = "开启后会打印大量日志，没遇到问题的话就关掉",
			DisplayName = "debug",
			SettingType = "Toggle",
			Key = "debug",
			DefaultValue = false
		},
		[14] = 
		{
			Description = [[重复添加怀孕锁会导致过月红字，打开激活保护逻辑
如有其他mod处理，则不用开启
如果没有其他触发怀孕的mod，也可以不打开]],
			DisplayName = "过月红字保护",
			SettingType = "Toggle",
			Key = "pregnantLockProtect",
			DefaultValue = false
		}
	},
	FileId = 2871617896,
	Author = "EveningTwilight",
	Description = [[姬友传说 v1.0.9
支持指定谷中密友和隐秘小村NPC的性向
支持设定新生儿男女比例、双性恋概率、同性生殖(及成功率)、是否生蛐蛐等
说是姬友传说，其实也可以是基佬传说
免责声明:
本mod仅为娱乐之用，不代表作者对各种性向的态度
也不带任何宣传教唆之意，未成年人请在家长陪同下使用

更新日志:
	v1.0.9 增加异常日志，对其他mod可能的冲突增加保护(目前已知 安居乐业mod有冲突，已处理)
	v1.0.8 修复 指定太吾对象 怀孕，指定失效
	v1.0.7 针对怀孕锁增加红字保护
	v1.0.6 新增 隐村姬友、春宵命中率 设置; 增加调试开关(控制日志)
	v1.0.5 及之前，不在工坊，懒得写]],
	Source = 1,
	HasArchive = false,
	Title = "姬友传说",
	Cover = "Cover.png",
	BackendPlugins = 
	{
		[1] = "LesLegends.dll"
	}
}