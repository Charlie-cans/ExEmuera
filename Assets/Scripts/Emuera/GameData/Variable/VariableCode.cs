using System;
using System.Collections.Generic;
using System.Text;

namespace MinorShift.Emuera.GameData.Variable
{
	//用于混淆的属性。如果进行enum.ToString()或enum.Parse()操作，请设置为(Exclude=true)。
	[global::System.Reflection.Obfuscation(Exclude=true)]
	internal enum VariableCode
	{
		__NULL__ = 0x00000000,
        __CAN_FORBID__ = 0x00010000,
		__INTEGER__ = 0x00020000,
		__STRING__ = 0x00040000,
		__ARRAY_1D__ = 0x00080000,
		__CHARACTER_DATA__ = 0x00100000,//第一引数を省略可能。TARGETで補う
		__UNCHANGEABLE__ = 0x00400000,//変更不可属性
		__CALC__ = 0x00800000,//計算値
		__EXTENDED__ = 0x01000000,//Emueraで追加した変数
		__LOCAL__ = 0x02000000,//局部变量。
		__GLOBAL__ = 0x04000000,//グローバル変数。
		__ARRAY_2D__ = 0x08000000,//二次元配列。キャラクタ変数标志と排他
		__SAVE_EXTENDED__ = 0x10000000,//拡張セーブ機能によってセーブするべき変数。
							//设置此标志后会自动保存（应该）。注意改名后将无法正常加载。
        __ARRAY_3D__ = 0x20000000,//三次元配列
        __CONSTANT__ = 0x40000000,//完全定数CSVから読み込まれる～NAME系がこれに該当

		__UPPERCASE__ = 0x7FFF0000,
		__LOWERCASE__ = 0x0000FFFF,

		__COUNT_SAVE_INTEGER__ = 0x00,//実は全て配列
		__COUNT_INTEGER__ = 0x00,
		//PALAMLV, EXPLV, RESULT, COUNT, TARGET, SELECTCOM不可设置禁止
		DAY = 0x00 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//经过天数。
		MONEY = 0x01 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//金钱
		ITEM = 0x02 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//持有数
		FLAG = 0x03 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//标志
		TFLAG = 0x04 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//一時标志
		UP = 0x05 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//调教中参数的上升值。index为PALAM.CSV中定义的值。
		PALAMLV = 0x06 | __INTEGER__ | __ARRAY_1D__,//调教中参数的分级阈值。超过阈值后珠的数量会增加。
		EXPLV = 0x07 | __INTEGER__ | __ARRAY_1D__,//经验的分级阈值。超过阈值后调教效果提升。
		EJAC = 0x08 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//用于射精检查的临时变量。
		DOWN = 0x09 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//调教中参数的减少值。index为PALAM.CSV中定义的值
		RESULT = 0x0A | __INTEGER__ | __ARRAY_1D__,//返回值(数值)
		COUNT = 0x0B | __INTEGER__ | __ARRAY_1D__,//循环计数器
		TARGET = 0x0C | __INTEGER__ | __ARRAY_1D__,//调教中角色的"注册编号"
		ASSI = 0x0D | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//助手角色的"注册编号"
		MASTER = 0x0E | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//主角角色的"注册编号"。通常为0
		NOITEM = 0x0F | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//物品是否存在？设置为不存在则为1。GAMEBASE.CSV
		LOSEBASE = 0x10 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//基础参数的减少值。通常LOSEBASE:0为体力消耗，LOSEBASE:1为精力消耗。
		SELECTCOM = 0x11 | __INTEGER__ | __ARRAY_1D__,//被选择的指令。与TRAIN.CSV中的相同
		ASSIPLAY = 0x12 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//助手当前是否正在调教？1 = true, 0 = false
		PREVCOM = 0x13 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//上一次的指令。
		NOTUSE_14 = 0x14 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//eramaker中存储RAND的区域。
		NOTUSE_15 = 0x15 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//eramaker中存储CHARANUM的区域。
		TIME = 0x16 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//时间
		ITEMSALES = 0x17 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//是否正在出售？
		PLAYER = 0x18 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//正在调教的人的角色的注册编号。通常为MASTER或ASSI
		NEXTCOM = 0x19 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//正在调教的人的角色的注册编号。通常为MASTER或ASSI
		PBAND = 0x1A | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//阴茎带的物品编号
		BOUGHT = 0x1B | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//最近购买的物品编号
		NOTUSE_1C = 0x1C | __INTEGER__ | __ARRAY_1D__,//未使用区域
		NOTUSE_1D = 0x1D | __INTEGER__ | __ARRAY_1D__,//未使用区域
		A = 0x1E | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,//通用变量
        B = 0x1F | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        C = 0x20 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        D = 0x21 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        E = 0x22 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        F = 0x23 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        G = 0x24 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        H = 0x25 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        I = 0x26 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        J = 0x27 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        K = 0x28 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        L = 0x29 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        M = 0x2A | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        N = 0x2B | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        O = 0x2C | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        P = 0x2D | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        Q = 0x2E | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        R = 0x2F | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        S = 0x30 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        T = 0x31 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        U = 0x32 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        V = 0x33 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        W = 0x34 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        X = 0x35 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        Y = 0x36 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
        Z = 0x37 | __INTEGER__ | __ARRAY_1D__ | __CAN_FORBID__,
		NOTUSE_38 = 0x38 | __INTEGER__ | __ARRAY_1D__,//未使用区域
		NOTUSE_39 = 0x39 | __INTEGER__ | __ARRAY_1D__,//未使用区域
		NOTUSE_3A = 0x3A | __INTEGER__ | __ARRAY_1D__,//未使用区域
		NOTUSE_3B = 0x3B | __INTEGER__ | __ARRAY_1D__,//未使用区域
		__COUNT_SAVE_INTEGER_ARRAY__ = 0x3C,

		ITEMPRICE = 0x3C | __INTEGER__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CAN_FORBID__,//物品价格
		LOCAL = 0x3D | __INTEGER__ | __ARRAY_1D__ | __LOCAL__ | __EXTENDED__ | __CAN_FORBID__,//局部变量
		ARG = 0x3E | __INTEGER__ | __ARRAY_1D__ | __LOCAL__ | __EXTENDED__ | __CAN_FORBID__,//函数参数用
		GLOBAL = 0x3F | __INTEGER__ | __ARRAY_1D__ | __GLOBAL__ | __EXTENDED__ | __CAN_FORBID__,//全局数值型变量
		RANDDATA = 0x40 | __INTEGER__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__,//全局数值型变量
		__COUNT_INTEGER_ARRAY__ = 0x41,


		SAVESTR = 0x00 | __STRING__ | __ARRAY_1D__ | __CAN_FORBID__,//字符串数据。会被保存
		__COUNT_SAVE_STRING_ARRAY__ = 0x01,


		//RESULTS不可设置禁止
		STR = 0x01 | __STRING__ | __ARRAY_1D__ | __CAN_FORBID__,//字符串数据。STR.CSV。可改写。
		RESULTS = 0x02 | __STRING__ | __ARRAY_1D__,//实际上这个也是数组
		LOCALS = 0x03 | __STRING__ | __ARRAY_1D__ | __LOCAL__ | __EXTENDED__ | __CAN_FORBID__, //局部字符串变量
		ARGS = 0x04 | __STRING__ | __ARRAY_1D__ | __LOCAL__ | __EXTENDED__ | __CAN_FORBID__,//函数参数用
		GLOBALS = 0x05 | __STRING__ | __ARRAY_1D__ | __GLOBAL__ | __EXTENDED__ | __CAN_FORBID__, //全局字符串变量
		TSTR = 0x06 | __STRING__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,

		__COUNT_STRING_ARRAY__ = 0x07,



		SAVEDATA_TEXT = 0x00 | __STRING__ | __EXTENDED__, //保存时使用的字符串。可通过PUTFORM添加
		__COUNT_SAVE_STRING__ = 0x00,
		__COUNT_STRING__ = 0x01,






		ISASSI = 0x00 | __INTEGER__ | __CHARACTER_DATA__,//是否为助手？1 = true, 0 = false
		NO = 0x01 | __INTEGER__ | __CHARACTER_DATA__,//角色编号

		__COUNT_SAVE_CHARACTER_INTEGER__ = 0x02,//这些似乎不是数组。
		__COUNT_CHARACTER_INTEGER__ = 0x02,

		BASE = 0x00 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//基础参数。
		MAXBASE = 0x01 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//基础参数的最大值。
		ABL = 0x02 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//能力。ABL.CSV
		TALENT = 0x03 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//素质。TALENT.CSV
		EXP = 0x04 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//经验。EXP.CSV
		MARK = 0x05 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//刻印。MARK.CSV
		PALAM = 0x06 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//调教中参数。PALAM.CSV
		SOURCE = 0x07 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//调教中参数。前一个指令产生的调教来源。
		EX = 0x08 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//调教中参数。本次调教中，在哪里高潮了几次。
		CFLAG = 0x09 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//标志。
		JUEL = 0x0A | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//珠。PALAM.CSV
		RELATION = 0x0B | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//関係。indexは登録番号ではなく角色编号
		EQUIP = 0x0C | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//未使用变量
		TEQUIP = 0x0D | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//调教中参数。是否正在使用物品。ITEM.CSV
		STAIN = 0x0E | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__,//调教中参数。污垢
		GOTJUEL = 0x0F | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//调教中参数。本次获得的珠。PALAM.CSV
		NOWEX = 0x10 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __CAN_FORBID__,//调教中参数。前一个指令中在哪里高潮了几次。
        DOWNBASE = 0x11 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__, //调教中参数。LOSEBASE的角色变量版
        CUP = 0x12 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,//调教中参数。UP的角色变量版
        CDOWN = 0x13 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,//调教中参数。DOWN的角色变量版
        TCVAR = 0x14 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,//角色变量中的临时变量


		__COUNT_SAVE_CHARACTER_INTEGER_ARRAY__ = 0x11,
		__COUNT_CHARACTER_INTEGER_ARRAY__ = 0x54,

		NAME = 0x00 | __STRING__ | __CHARACTER_DATA__,//名字//通过注册编号调用
		CALLNAME = 0x01 | __STRING__ | __CHARACTER_DATA__,//称呼
		NICKNAME = 0x02 | __STRING__ | __CHARACTER_DATA__ | __SAVE_EXTENDED__ | __EXTENDED__,//昵称
		MASTERNAME = 0x03 | __STRING__ | __CHARACTER_DATA__ | __SAVE_EXTENDED__ | __EXTENDED__,//昵称

		__COUNT_SAVE_CHARACTER_STRING__ = 0x02,
		__COUNT_CHARACTER_STRING__ = 0x04,

		CSTR = 0x00 | __STRING__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,//角色用字符串数组

		__COUNT_SAVE_CHARACTER_STRING_ARRAY__ = 0x00,
		__COUNT_CHARACTER_STRING_ARRAY__ = 0x01,

		CDFLAG = 0x00 | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,

		__COUNT_CHARACTER_INTEGER_ARRAY_2D__ = 0x01,

		__COUNT_CHARACTER_STRING_ARRAY_2D__ = 0x00,


		DITEMTYPE = 0x00 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DA = 0x01 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DB = 0x02 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DC = 0x03 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DD = 0x04 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
		DE = 0x05 | __INTEGER__ | __ARRAY_2D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
        __COUNT_INTEGER_ARRAY_2D__ = 0x06,

		__COUNT_STRING_ARRAY_2D__ = 0x00,

		TA = 0x00 | __INTEGER__ | __ARRAY_3D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
        TB = 0x01 | __INTEGER__ | __ARRAY_3D__ | __SAVE_EXTENDED__ | __EXTENDED__ | __CAN_FORBID__,
        __COUNT_INTEGER_ARRAY_3D__ = 0x02,

        __COUNT_STRING_ARRAY_3D__ = 0x00,

		//对于CALC类变量，编号顺序无关紧要。
		//1803beta004 ～～NAME系的编号顺序由ConstantData使用，因此很重要
		
		RAND = 0x00 | __INTEGER__ | __ARRAY_1D__ | __CALC__ | __UNCHANGEABLE__,//随机数。返回0到参数-1之间的值。
		CHARANUM = 0x01 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__,//角色数量。返回角色注册数量。

		ABLNAME = 0x00 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//能力。ABL.CSV//csvから読まれるデータは保存されない。変更不可
		EXPNAME = 0x01 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//经验。EXP.CSV
		TALENTNAME = 0x02 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//素质。TALENT.CSV
		PALAMNAME = 0x03 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//能力。PALAM.CSV
		TRAINNAME = 0x04 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//調教名。TRAIN.CSV
		MARKNAME = 0x05 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//刻印。MARK.CSV
		ITEMNAME = 0x06 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __CONSTANT__ | __CAN_FORBID__,//アイテム。ITEM.CSV
		BASENAME = 0x07 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//基礎能力名。BASE.CSV
		SOURCENAME = 0x08 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//調教ソース名。SOURCE.CSV
		EXNAME = 0x09 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//絶頂名。EX.CSV
		__DUMMY_STR__ = 0x0A | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__,
		EQUIPNAME = 0x0B | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//装着物名。EQUIP.CSV
		TEQUIPNAME = 0x0C | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//調教時装着物名。TEQUIP.CSV
		FLAGNAME = 0x0D | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//标志名。FLAG.CSV
		TFLAGNAME = 0x0E | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//一時标志名。TFLAG.CSV
		CFLAGNAME = 0x0F | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,//キャラクタ标志名。CFLAG.CSV
		TCVARNAME = 0x10 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		CSTRNAME = 0x11 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		STAINNAME = 0x12 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,

		CDFLAGNAME1 = 0x13 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		CDFLAGNAME2 = 0x14 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		STRNAME = 0x15 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		TSTRNAME = 0x16 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		SAVESTRNAME = 0x17 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		GLOBALNAME = 0x18 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,
		GLOBALSNAME = 0x19 | __STRING__ | __ARRAY_1D__ | __UNCHANGEABLE__ | __EXTENDED__ | __CONSTANT__ | __CAN_FORBID__,

        __COUNT_CSV_STRING_ARRAY_1D__ = 0x1A,


		GAMEBASE_AUTHER = 0x04 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//字符串型。作者。拼写有误但为了兼容性保留。
		GAMEBASE_AUTHOR = 0x00 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//字符串型。作者
		GAMEBASE_INFO = 0x01 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//字符串型。附加信息
		GAMEBASE_YEAR = 0x02 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//字符串型。制作年份
		GAMEBASE_TITLE = 0x03 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//字符串型。标题
		WINDOW_TITLE = 0x05 | __STRING__ | __CALC__ | __EXTENDED__,//字符串型。窗口标题。可修改。
		//添加用双下划线包围的变量时，VariableToken需要进行特殊处理。
		__FILE__ = 0x06 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//当前执行中的文件名
		__FUNCTION__ = 0x07 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//当前执行中的函数名
        MONEYLABEL = 0x08 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//お金钱のラベル
        DRAWLINESTR = 0x09 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//DRAWLINE的绘制字符串
        EMUERA_VERSION = 0x0A | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__, //Emeura的版本

		LASTLOAD_TEXT = 0x05 | __STRING__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数值型。

		GAMEBASE_GAMECODE = 0x00 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数值型。コード
		GAMEBASE_VERSION = 0x01 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数值型。バージョン
		GAMEBASE_ALLOWVERSION = 0x02 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数值型。バージョン違い認める
		GAMEBASE_DEFAULTCHARA = 0x03 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数值型。最初からいるキャラ
		GAMEBASE_NOITEM = 0x04 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数值型。アイテムなし

		LASTLOAD_VERSION = 0x05 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数值型。
		LASTLOAD_NO = 0x06 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//数值型。
		__LINE__ = 0x07 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//当前执行中的行号
		LINECOUNT = 0x08 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//绘制的行总数。通过CLEAR减少
        ISTIMEOUT = 0x0B | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//TINPUT系等是否超时？

        __INT_MAX__ = 0x09 | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//Int64最大值
        __INT_MIN__ = 0x0A | __INTEGER__ | __CALC__ | __UNCHANGEABLE__ | __EXTENDED__,//Int64最小值

		CVAR = 0xFC | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __EXTENDED__,//用户定义变量
		CVARS = 0xFC | __STRING__ | __CHARACTER_DATA__ | __ARRAY_1D__ | __EXTENDED__,//用户定义变量
		CVAR2D = 0xFC | __INTEGER__ | __CHARACTER_DATA__ | __ARRAY_2D__ | __EXTENDED__,//用户定义变量
		CVARS2D = 0xFC | __STRING__ | __CHARACTER_DATA__ | __ARRAY_2D__ | __EXTENDED__,//用户定义变量
		//CVAR3D = 0xFC | __INTEGER__ | __ARRAY_3D__ | __EXTENDED__,//用户定义变量
		//CVARS3D = 0xFC | __STRING__ | __ARRAY_3D__ | __EXTENDED__,//用户定义变量
		REF = 0xFD | __INTEGER__ | __ARRAY_1D__ | __EXTENDED__,//引用类型
		REFS = 0xFD | __STRING__ | __ARRAY_1D__ | __EXTENDED__,
		REF2D = 0xFD | __INTEGER__ | __ARRAY_2D__ | __EXTENDED__,
		REFS2D = 0xFD | __STRING__ | __ARRAY_2D__ | __EXTENDED__,
		REF3D = 0xFD | __INTEGER__ | __ARRAY_3D__ | __EXTENDED__,
		REFS3D = 0xFD | __STRING__ | __ARRAY_3D__ | __EXTENDED__,
		VAR = 0xFE | __INTEGER__ | __ARRAY_1D__ | __EXTENDED__,//用户定义变量 1808 不区分私有变量和全局变量
		VARS = 0xFE | __STRING__ | __ARRAY_1D__ | __EXTENDED__,//用户定义变量
		VAR2D = 0xFE | __INTEGER__ | __ARRAY_2D__ | __EXTENDED__,//用户定义变量
		VARS2D = 0xFE | __STRING__ | __ARRAY_2D__ | __EXTENDED__,//用户定义变量
		VAR3D = 0xFE | __INTEGER__ | __ARRAY_3D__ | __EXTENDED__,//用户定义变量
		VARS3D = 0xFE | __STRING__ | __ARRAY_3D__ | __EXTENDED__,//用户定义变量
		//PRIVATE = 0xFF | __INTEGER__ | __ARRAY_1D__ | __EXTENDED__,//私有变量
		//PRIVATES = 0xFF | __STRING__ | __ARRAY_1D__ | __EXTENDED__,//私有变量
		//PRIVATE2D = 0xFF | __INTEGER__ | __ARRAY_2D__ | __EXTENDED__,//私有变量
		//PRIVATES2D = 0xFF | __STRING__ | __ARRAY_2D__ | __EXTENDED__,//私有变量
		//PRIVATE3D = 0xFF | __INTEGER__ | __ARRAY_3D__ | __EXTENDED__,//私有变量
		//PRIVATES3D = 0xFF | __STRING__ | __ARRAY_3D__ | __EXTENDED__,//私有变量
	}
}

