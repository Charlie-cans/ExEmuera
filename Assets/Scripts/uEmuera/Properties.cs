using System.Collections.Generic;

namespace Properties
{
    public static class ResourceManager
    {
        static Dictionary<string, string> dict = new Dictionary<string, string>
        {
            { "RuntimeErrMesMethodCIMGCreateOutOfRange0","{0}関数:画像の範囲外が指定されています"},
            { "RuntimeErrMesMethodColorARGB0","{0}関数:ColorARGB引数に不適切な値(0x{1:X8})が指定されました"},
            { "RuntimeErrMesMethodDefaultArgumentOutOfRange0","{0}関数:第{2}引数に不適切な値({1})が指定されました"},
            { "RuntimeErrMesMethodGColorMatrix0","{0}関数:ColorMatrixの指定された要素({1}, {2})が不適切であるか5x5に足りていません"},
            { "RuntimeErrMesMethodGDIPLUSOnly","{0}関数:描画オプションがWINAPIの時には使用できません"},
            { "RuntimeErrMesMethodGHeight0","{0}関数:GraphicsのHeightに0以下の値({1})が指定されました"},
            { "RuntimeErrMesMethodGHeight1","{0}関数:GraphicsのHeightに{2}以上の値({1})が指定されました"},
            { "RuntimeErrMesMethodGraphicsID0","{0}関数:GraphicsIDに負の値({1})が指定されました"},
            { "RuntimeErrMesMethodGraphicsID1","{0}関数:GraphicsIDの値({1})が大きすぎます"},
            { "RuntimeErrMesMethodGWidth0","{0}関数:GraphicsのWidthに0以下の値({1})が指定されました"},
            { "RuntimeErrMesMethodGWidth1","{0}関数:GraphicsのWidthに{2}以上の値({1})が指定されました"},
            { "SyntaxErrMesMethodDefaultArgumentNotNullable0","{0}関数:第{1}引数は省略できません"},
            { "SyntaxErrMesMethodDefaultArgumentNum0","{0}関数:引数の数が間違っています"},
            { "SyntaxErrMesMethodDefaultArgumentNum1","{0}関数:少なくとも{1}個の引数が必要です"},
            { "SyntaxErrMesMethodDefaultArgumentNum2","{0}関数:引数の数が多すぎます"},
            { "SyntaxErrMesMethodDefaultArgumentType0","{0}関数:第{1}引数の型が間違っています"},
            { "SyntaxErrMesMethodGraphicsColorMatrix0","{0}関数:ColorMatrixに5x5以上の二次元数値型配列変数でない引数が指定されました"},
        };

        public static string GetString(string key, object culture)
        {
            string s;
            dict.TryGetValue(key, out s);
            return s;
        }
    }

    public static class Resources
    {
        private static global::System.Globalization.CultureInfo resourceCulture;

        /// <summary>
        ///   {0}函数:指定的图像范围超出边界 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodCIMGCreateOutOfRange0
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodCIMGCreateOutOfRange0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:ColorARGB参数指定了不适当的值(0x{1:X8}) 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodColorARGB0
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodColorARGB0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:第{2}个参数指定了不适当的值({1}) 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodDefaultArgumentOutOfRange0
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodDefaultArgumentOutOfRange0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:ColorMatrix指定的元素({1}, {2})不适当或不足5x5 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodGColorMatrix0
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodGColorMatrix0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:绘图选项为WINAPI时无法使用 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodGDIPLUSOnly
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodGDIPLUSOnly", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:为Graphics的Height指定了0以下的值({1}) 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodGHeight0
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodGHeight0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:为Graphics的Height指定了{2}以上的值({1}) 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodGHeight1
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodGHeight1", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:为GraphicsID指定了负值({1}) 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodGraphicsID0
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodGraphicsID0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:GraphicsID的值({1})过大 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodGraphicsID1
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodGraphicsID1", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:为Graphics的Width指定了0以下的值({1}) 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodGWidth0
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodGWidth0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:为Graphics的Width指定了{2}以上的值({1}) 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string RuntimeErrMesMethodGWidth1
        {
            get
            {
                return ResourceManager.GetString("RuntimeErrMesMethodGWidth1", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:第{1}个参数不可省略 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string SyntaxErrMesMethodDefaultArgumentNotNullable0
        {
            get
            {
                return ResourceManager.GetString("SyntaxErrMesMethodDefaultArgumentNotNullable0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:参数数量不正确 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string SyntaxErrMesMethodDefaultArgumentNum0
        {
            get
            {
                return ResourceManager.GetString("SyntaxErrMesMethodDefaultArgumentNum0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:至少需要{1}个参数 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string SyntaxErrMesMethodDefaultArgumentNum1
        {
            get
            {
                return ResourceManager.GetString("SyntaxErrMesMethodDefaultArgumentNum1", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:参数数量过多 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string SyntaxErrMesMethodDefaultArgumentNum2
        {
            get
            {
                return ResourceManager.GetString("SyntaxErrMesMethodDefaultArgumentNum2", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:第{1}个参数的类型不正确 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string SyntaxErrMesMethodDefaultArgumentType0
        {
            get
            {
                return ResourceManager.GetString("SyntaxErrMesMethodDefaultArgumentType0", resourceCulture);
            }
        }

        /// <summary>
        ///   {0}函数:为ColorMatrix指定了非5x5以上二维数值型数组变量的参数 搜索与上述相似的本地化字符串。
        /// </summary>
        public static string SyntaxErrMesMethodGraphicsColorMatrix0
        {
            get
            {
                return ResourceManager.GetString("SyntaxErrMesMethodGraphicsColorMatrix0", resourceCulture);
            }
        }
    }
}
