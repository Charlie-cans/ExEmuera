using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameProc;


namespace MinorShift.Emuera.GameData.Function
{
	internal static partial class FunctionMethodCreator
	{
		static FunctionMethodCreator()
		{
            methodList = new Dictionary<string, FunctionMethod>
            {
                //キャラクタデータ系
                ["GETCHARA"] = new GetcharaMethod(),
                ["GETSPCHARA"] = new GetspcharaMethod(),
                ["CSVNAME"] = new CsvStrDataMethod(CharacterStrData.NAME),
                ["CSVCALLNAME"] = new CsvStrDataMethod(CharacterStrData.CALLNAME),
                ["CSVNICKNAME"] = new CsvStrDataMethod(CharacterStrData.NICKNAME),
                ["CSVMASTERNAME"] = new CsvStrDataMethod(CharacterStrData.MASTERNAME),
                ["CSVCSTR"] = new CsvcstrMethod(),
                ["CSVBASE"] = new CsvDataMethod(CharacterIntData.BASE),
                ["CSVABL"] = new CsvDataMethod(CharacterIntData.ABL),
                ["CSVMARK"] = new CsvDataMethod(CharacterIntData.MARK),
                ["CSVEXP"] = new CsvDataMethod(CharacterIntData.EXP),
                ["CSVRELATION"] = new CsvDataMethod(CharacterIntData.RELATION),
                ["CSVTALENT"] = new CsvDataMethod(CharacterIntData.TALENT),
                ["CSVCFLAG"] = new CsvDataMethod(CharacterIntData.CFLAG),
                ["CSVEQUIP"] = new CsvDataMethod(CharacterIntData.EQUIP),
                ["CSVJUEL"] = new CsvDataMethod(CharacterIntData.JUEL),
                ["FINDCHARA"] = new FindcharaMethod(false),
                ["FINDLASTCHARA"] = new FindcharaMethod(true),
                ["EXISTCSV"] = new ExistCsvMethod(),

                //汎用処理系
                ["VARSIZE"] = new VarsizeMethod(),
                ["CHKFONT"] = new CheckfontMethod(),
                ["CHKDATA"] = new CheckdataMethod(EraSaveFileType.Normal),
                ["ISSKIP"] = new IsSkipMethod(),
                ["MOUSESKIP"] = new MesSkipMethod(true),
                ["MESSKIP"] = new MesSkipMethod(false),
                ["GETCOLOR"] = new GetColorMethod(false),
                ["GETDEFCOLOR"] = new GetColorMethod(true),
                ["GETFOCUSCOLOR"] = new GetFocusColorMethod(),
                ["GETBGCOLOR"] = new GetBGColorMethod(false),
                ["GETDEFBGCOLOR"] = new GetBGColorMethod(true),
                ["GETSTYLE"] = new GetStyleMethod(),
                ["GETFONT"] = new GetFontMethod(),
                ["BARSTR"] = new BarStringMethod(),
                ["CURRENTALIGN"] = new CurrentAlignMethod(),
                ["CURRENTREDRAW"] = new CurrentRedrawMethod(),
                ["COLOR_FROMNAME"] = new ColorFromNameMethod(),
                ["COLOR_FROMRGB"] = new ColorFromRGBMethod(),

                //TODO:1810
                //methodList["CHKVARDATA"] = new CheckdataStrMethod(EraSaveFileType.Var);
                ["CHKCHARADATA"] = new CheckdataStrMethod(EraSaveFileType.CharVar),
                //methodList["CHKGLOBALDATA"] = new CheckdataMethod(EraSaveFileType.Global);
                //methodList["FIND_VARDATA"] = new FindFilesMethod(EraSaveFileType.Var);
                ["FIND_CHARADATA"] = new FindFilesMethod(EraSaveFileType.CharVar),

                //定数取得
                ["MONEYSTR"] = new MoneyStrMethod(),
                ["PRINTCPERLINE"] = new GetPrintCPerLineMethod(),
                ["PRINTCLENGTH"] = new PrintCLengthMethod(),
                ["SAVENOS"] = new GetSaveNosMethod(),
                ["GETTIME"] = new GettimeMethod(),
                ["GETTIMES"] = new GettimesMethod(),
                ["GETMILLISECOND"] = new GetmsMethod(),
                ["GETSECOND"] = new GetSecondMethod(),

                //数学関数
                ["RAND"] = new RandMethod(),
                ["MIN"] = new MaxMethod(false),
                ["MAX"] = new MaxMethod(true),
                ["ABS"] = new AbsMethod(),
                ["POWER"] = new PowerMethod(),
                ["SQRT"] = new SqrtMethod(),
                ["CBRT"] = new CbrtMethod(),
                ["LOG"] = new LogMethod(),
                ["LOG10"] = new LogMethod(10.0d),
                ["EXPONENT"] = new ExpMethod(),
                ["SIGN"] = new SignMethod(),
                ["LIMIT"] = new GetLimitMethod(),

                //変数操作系
                ["SUMARRAY"] = new SumArrayMethod(),
                ["SUMCARRAY"] = new SumArrayMethod(true),
                ["MATCH"] = new MatchMethod(),
                ["CMATCH"] = new MatchMethod(true),
                ["GROUPMATCH"] = new GroupMatchMethod(),
                ["NOSAMES"] = new NosamesMethod(),
                ["ALLSAMES"] = new AllsamesMethod(),
                ["MAXARRAY"] = new MaxArrayMethod(),
                ["MAXCARRAY"] = new MaxArrayMethod(true),
                ["MINARRAY"] = new MaxArrayMethod(false, false),
                ["MINCARRAY"] = new MaxArrayMethod(true, false),
                ["GETBIT"] = new GetbitMethod(),
                ["GETNUM"] = new GetnumMethod(),
                ["GETPALAMLV"] = new GetPalamLVMethod(),
                ["GETEXPLV"] = new GetExpLVMethod(),
                ["FINDELEMENT"] = new FindElementMethod(false),
                ["FINDLASTELEMENT"] = new FindElementMethod(true),
                ["INRANGE"] = new InRangeMethod(),
                ["INRANGEARRAY"] = new InRangeArrayMethod(),
                ["INRANGECARRAY"] = new InRangeArrayMethod(true),
                ["GETNUMB"] = new GetnumMethod(),

                ["ARRAYMSORT"] = new ArrayMultiSortMethod(),

                //文字列操作系
                ["STRLENS"] = new StrlenMethod(),
                ["STRLENSU"] = new StrlenuMethod(),
                ["SUBSTRING"] = new SubstringMethod(),
                ["SUBSTRINGU"] = new SubstringuMethod(),
                ["STRFIND"] = new StrfindMethod(false),
                ["STRFINDU"] = new StrfindMethod(true),
                ["STRCOUNT"] = new StrCountMethod(),
                ["TOSTR"] = new ToStrMethod(),
                ["TOINT"] = new ToIntMethod(),
                ["TOUPPER"] = new StrChangeStyleMethod(StrFormType.Upper),
                ["TOLOWER"] = new StrChangeStyleMethod(StrFormType.Lower),
                ["TOHALF"] = new StrChangeStyleMethod(StrFormType.Half),
                ["TOFULL"] = new StrChangeStyleMethod(StrFormType.Full),
                ["LINEISEMPTY"] = new LineIsEmptyMethod(),
                ["REPLACE"] = new ReplaceMethod(),
                ["UNICODE"] = new UnicodeMethod(),
                ["UNICODEBYTE"] = new UnicodeByteMethod(),
                ["CONVERT"] = new ConvertIntMethod(),
                ["ISNUMERIC"] = new IsNumericMethod(),
                ["ESCAPE"] = new EscapeMethod(),
                ["ENCODETOUNI"] = new EncodeToUniMethod(),
                ["CHARATU"] = new CharAtMethod(),
                ["GETLINESTR"] = new GetLineStrMethod(),
                ["STRFORM"] = new StrFormMethod(),
                ["STRJOIN"] = new JoinMethod(),

                ["GETCONFIG"] = new GetConfigMethod(true),
                ["GETCONFIGS"] = new GetConfigMethod(false),

                //html系
                ["HTML_GETPRINTEDSTR"] = new HtmlGetPrintedStrMethod(),
                ["HTML_POPPRINTINGSTR"] = new HtmlPopPrintingStrMethod(),
                ["HTML_TOPLAINTEXT"] = new HtmlToPlainTextMethod(),
                ["HTML_ESCAPE"] = new HtmlEscapeMethod(),


                //画像処理系
                ["SPRITECREATED"] = new SpriteStateMethod(),
                ["SPRITEWIDTH"] = new SpriteStateMethod(),
                ["SPRITEHEIGHT"] = new SpriteStateMethod(),
                ["SPRITEMOVE"] = new SpriteSetPosMethod(),
                ["SPRITESETPOS"] = new SpriteSetPosMethod(),
                ["SPRITEPOSX"] = new SpriteStateMethod(),
                ["SPRITEPOSY"] = new SpriteStateMethod(),

                ["CLIENTWIDTH"] = new ClientSizeMethod(),
                ["CLIENTHEIGHT"] = new ClientSizeMethod(),

                ["GETKEY"] = new GetKeyStateMethod(),
                ["GETKEYTRIGGERED"] = new GetKeyStateMethod(),
                ["MOUSEX"] = new MousePosMethod(),
                ["MOUSEY"] = new MousePosMethod(),
                ["ISACTIVE"] = new IsActiveMethod(),
                ["SAVETEXT"] = new SaveTextMethod(),
                ["LOADTEXT"] = new LoadTextMethod(),

                ["GCREATED"] = new GraphicsStateMethod(),// ("GCREATED");
                ["GWIDTH"] = new GraphicsStateMethod(),//("GWIDTH");
                ["GHEIGHT"] = new GraphicsStateMethod(),//("GHEIGHT");
                ["GGETCOLOR"] = new GraphicsGetColorMethod(),
                ["SPRITEGETCOLOR"] = new SpriteGetColorMethod(),

                ["GCREATE"] = new GraphicsCreateMethod(),
                ["GCREATEFROMFILE"] = new GraphicsCreateFromFileMethod(),
                ["GDISPOSE"] = new GraphicsDisposeMethod(),
                ["GCLEAR"] = new GraphicsClearMethod(),
                ["GFILLRECTANGLE"] = new GraphicsFillRectangleMethod(),
                ["GDRAWSPRITE"] = new GraphicsDrawSpriteMethod(),
                ["GSETCOLOR"] = new GraphicsSetColorMethod(),
                ["GDRAWG"] = new GraphicsDrawGMethod(),
                ["GDRAWGWITHMASK"] = new GraphicsDrawGWithMaskMethod(),

                ["GSETBRUSH"] = new GraphicsSetBrushMethod(),
                ["GSETFONT"] = new GraphicsSetFontMethod(),
                ["GSETPEN"] = new GraphicsSetPenMethod(),

                ["SPRITECREATE"] = new SpriteCreateMethod(),
                ["SPRITEDISPOSE"] = new SpriteDisposeMethod(),

                ["CBGSETG"] = new CBGSetGraphicsMethod(),
                ["CBGSETSPRITE"] = new CBGSetCIMGMethod(),
                ["CBGCLEAR"] = new CBGClearMethod(),

                ["CBGCLEARBUTTON"] = new CBGClearButtonMethod(),
                ["CBGREMOVERANGE"] = new CBGRemoveRangeMethod(),
                ["CBGREMOVEBMAP"] = new CBGRemoveBMapMethod(),
                ["CBGSETBMAPG"] = new CBGSetBMapGMethod(),
                ["CBGSETBUTTONSPRITE"] = new CBGSETButtonSpriteMethod(),

                ["GSAVE"] = new GraphicsSaveMethod(),
                ["GLOAD"] = new GraphicsLoadMethod(),


                ["SPRITEANIMECREATE"] = new SpriteAnimeCreateMethod(),
                ["SPRITEANIMEADDFRAME"] = new SpriteAnimeAddFrameMethod(),
                ["SETANIMETIMER"] = new SetAnimeTimerMethod(),

                // EM+EE DataTable
                ["DT_CREATE"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Create),
                ["DT_EXIST"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Check),
                ["DT_RELEASE"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Release),
                ["DT_NOCASE"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Case),
                ["DT_CLEAR"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Clear),
                // T_ prefixed aliases for game compatibility
                ["T_CREATE"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Create),
                ["T_EXIST"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Check),
                ["T_RELEASE"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Release),
                ["T_NOCASE"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Case),
                ["T_CLEAR"] = new DataTableManagementMethod(DataTableManagementMethod.Operation.Clear),
                ["DT_COLUMN_ADD"] = new DataTableColumnManagementMethod(DataTableColumnManagementMethod.Operation.Create),
                ["DT_COLUMN_NAMES"] = new DataTableColumnManagementMethod(DataTableColumnManagementMethod.Operation.Names),
                ["DT_COLUMN_EXIST"] = new DataTableColumnManagementMethod(DataTableColumnManagementMethod.Operation.Check),
                ["DT_COLUMN_REMOVE"] = new DataTableColumnManagementMethod(DataTableColumnManagementMethod.Operation.Remove),
                ["DT_COLUMN_LENGTH"] = new DataTableLengthMethod(DataTableLengthMethod.Operation.Column),
                ["DT_ROW_ADD"] = new DataTableRowSetMethod(DataTableRowSetMethod.Operation.Add),
                ["DT_ROW_SET"] = new DataTableRowSetMethod(DataTableRowSetMethod.Operation.Set),
                ["DT_ROW_REMOVE"] = new DataTableRowRemoveMethod(),
                ["DT_ROW_LENGTH"] = new DataTableLengthMethod(DataTableLengthMethod.Operation.Row),
                ["DT_CELL_GET"] = new DataTableCellGetMethod(DataTableCellGetMethod.Operation.Get),
                ["DT_CELL_ISNULL"] = new DataTableCellGetMethod(DataTableCellGetMethod.Operation.IsNull),
                ["DT_CELL_GETS"] = new DataTableCellGetMethod(DataTableCellGetMethod.Operation.Gets),
                ["DT_CELL_SET"] = new DataTableCellSetMethod(),
                ["DT_SELECT"] = new DataTableSelectMethod(),
                ["DT_TOXML"] = new DataTableToXmlMethod(),
                ["DT_FROMXML"] = new DataTableFromXmlMethod(),

                // EM+EE Misc
                ["CLEARMEMORY"] = new ClearMemoryMethod(),
                ["EXISTSOUND"] = new ExistSoundMethod(),
                ["EXISTFILE"] = new ExistFileMethod(),
                ["GETVAR"] = new GetVarMethod(false),
                ["GETVARS"] = new GetVarMethod(true),
                ["SETVAR"] = new SetVarMethod(),
                ["ERDNAME"] = new ErdNameMethod(),
                ["SPRITEDISPOSEALL"] = new SpriteDisposeAllMethod(),
                ["GETDISPLAYLINE"] = new GetDisplayLineMethod(),
                ["MOVETEXTBOX"] = new MoveTextBoxMethod(false),
                ["RESUMETEXTBOX"] = new MoveTextBoxMethod(true),
                ["GETTEXTBOX"] = new GetTextBoxMethod(),
                ["SETTEXTBOX"] = new ChangeTextBoxMethod(),
                ["GETDOINGFUNCTION"] = new GetDoingFunctionMethod(),

                // EM+EE SQL stubs
                ["SQL_CONNECT"] = new SqlStubMethod(1),
                ["SQL_DISCONNECT"] = new SqlStubMethod(1),
                ["SQL_EXECUTE"] = new SqlStubMethod(2),
                ["SQL_EXECUTE_SCALAR"] = new SqlStubMethod(2),
                ["SQL_EXECUTE_SCALAR_S"] = new SqlStubMethod(2),
                ["SQL_EXECUTE_READER"] = new SqlStubMethod(2),
                ["SQL_EXECUTE_NON_QUERY"] = new SqlStubMethod(2),
                ["SQL_IMPORT_MAP_XML"] = new SqlStubMethod(2),
                ["SQL_EXPORT_MAP_XML"] = new SqlStubMethod(2),
                ["SQL_IMPORT_DT_XML"] = new SqlStubMethod(2),
                ["SQL_EXPORT_DT_XML"] = new SqlStubMethod(2),
                ["SQL_READER_READ"] = new SqlStubMethod(1),
                ["SQL_READER_GET_STRING"] = new SqlStubMethod(2),
                ["SQL_READER_GET_LONG"] = new SqlStubMethod(2),
                ["SQL_READER_IS_NULL"] = new SqlStubMethod(1),
                ["SQL_READER_CLOSE"] = new SqlStubMethod(1),
                ["SQL_ESCAPE"] = new SqlStubMethod(1),

                // EM+EE MAP
                ["MAP_CREATE"] = new MapManagementMethod(MapManagementMethod.Operation.Create),
                ["MAP_EXIST"] = new MapManagementMethod(MapManagementMethod.Operation.Check),
                ["MAP_RELEASE"] = new MapManagementMethod(MapManagementMethod.Operation.Release),
                ["MAP_GET"] = new MapGetStrMethod(MapGetStrMethod.Operation.Get),
                ["MAP_CLEAR"] = new MapDataOperationMethod(MapDataOperationMethod.Operation.Clear),
                ["MAP_SIZE"] = new MapDataOperationMethod(MapDataOperationMethod.Operation.Size),
                ["MAP_HAS"] = new MapDataOperationMethod(MapDataOperationMethod.Operation.Has),
                ["MAP_SET"] = new MapDataOperationMethod(MapDataOperationMethod.Operation.Set),
                ["MAP_REMOVE"] = new MapDataOperationMethod(MapDataOperationMethod.Operation.Remove),
                ["MAP_GETKEYS"] = new MapGetStrMethod(MapGetStrMethod.Operation.GetKeys),

                // EM+EE HTML
                ["HTML_STRINGLEN"] = new HtmlStringLenStub(),
                ["HTML_SUBSTRING"] = new HtmlSubStringStub(),
                ["HTML_STRINGLINES"] = new HtmlStringLinesStub(),

                // EM+EE XML stubs
                ["XML_DOCUMENT"] = new XmlStubMethod("XML_DOCUMENT"),
                ["XML_RELEASE"] = new XmlStubMethod("XML_RELEASE"),
                ["XML_GET"] = new XmlStubMethod("XML_GET"),
                ["XML_SET"] = new XmlStubMethod("XML_SET"),
                ["XML_EXIST"] = new XmlStubMethod("XML_EXIST"),
                ["XML_TOSTR"] = new XmlStubMethod("XML_TOSTR"),
                ["XML_ADDNODE"] = new XmlStubMethod("XML_ADDNODE"),
                ["XML_REMOVENODE"] = new XmlStubMethod("XML_REMOVENODE"),
                ["XML_REMOVENODE_BYNAME"] = new XmlStubMethod("XML_REMOVENODE_BYNAME"),
                ["XML_REPLACE"] = new XmlStubMethod("XML_REPLACE"),
                ["XML_REPLACE_BYNAME"] = new XmlStubMethod("XML_REPLACE_BYNAME"),
                ["XML_ADDATTRIBUTE"] = new XmlStubMethod("XML_ADDATTRIBUTE"),
                ["XML_ADDATTRIBUTE_BYNAME"] = new XmlStubMethod("XML_ADDATTRIBUTE_BYNAME"),
                ["XML_REMOVEATTRIBUTE"] = new XmlStubMethod("XML_REMOVEATTRIBUTE"),
                ["XML_REMOVEATTRIBUTE_BYNAME"] = new XmlStubMethod("XML_REMOVEATTRIBUTE_BYNAME"),
                // EM+EE More
                ["VARSETEX"] = new SqlStubMethod(2),
                ["ARRAYMSORTEX"] = new SqlStubMethod(5),
                ["GDRAWTEXT"] = new SqlStubMethod(5),
                ["GETMETH"] = new PluginIntStubMethod(),
                ["GETMETHS"] = new PluginStrStubMethod(),
                ["EXISTMETH"] = new PluginIntStubMethod(),
                ["EXISTFUNCTION"] = new ExistFunctionStub(),
                ["REGEXPMATCH"] = new RegexStubMethod(),
                ["HTML_TOPLAINTEXT"] = new HtmlToPlainTextStub(),
                ["HTML_GETPRINTEDSTR"] = new HtmlGetPrintedStrStub(),
                ["HTML_POPPRINTINGSTR"] = new HtmlPopPrintingStrStub(),
                ["HOTKEY_STATE"] = new SqlStubMethod(1),
                ["BITMAP_CACHE_ENABLE"] = new SqlStubMethod(1),
                ["GDRAWLINE"] = new SqlStubMethod(4),
                ["GDASHSTYLE"] = new SqlStubMethod(1),
            };


            //1823 自分の関数名を知っていた方が何かと便利なので覚えさせることにした
            foreach (var pair in methodList)
				pair.Value.SetMethodName(pair.Key);
        }

		private static readonly Dictionary<string, FunctionMethod> methodList;
		public static Dictionary<string, FunctionMethod> GetMethodList()
		{
			return methodList;
		}
	}
}