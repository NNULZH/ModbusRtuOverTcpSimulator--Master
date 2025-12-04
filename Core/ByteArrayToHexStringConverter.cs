using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data; // 必须引用：用于 IValueConverter
//转换器用法: 声明在对应xmal资源中,通过资源使用!!!
namespace Core
{
    // [ValueConversion(typeof(byte[]), typeof(string))] // 可选：用于设计时提示
    public class ByteArrayToHexStringConverter : IValueConverter
    {
        /// <summary>
        /// 将 byte[] 转换为 Hex 字符串 (例如: "01 03 00 00")
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                // BitConverter 生成的是 "01-03-00"，我们将 "-" 替换为空格
                return BitConverter.ToString(bytes).Replace("-", " ");
            }
            return string.Empty;
        }

        /// <summary>
        /// 将 Hex 字符串转换为 byte[] (用于输入框绑定回源数据)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;

            if (string.IsNullOrWhiteSpace(str))
                return null;

            try
            {
                // 1. 清理字符串：移除空格、横杠、0x前缀等干扰字符
                string cleanHex = str.Replace(" ", "")
                                     .Replace("-", "")
                                     .Replace(":", "")
                                     .Replace("0x", "")
                                     .Trim();

                // 2. 检查长度，如果是奇数，无法转换（Hex是两字符一个字节）
                if (cleanHex.Length % 2 != 0)
                    return null; // 或者抛出异常，或者返回 Binding.DoNothing

                // 3. 解析为字节数组
                byte[] bytes = new byte[cleanHex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    // 每2个字符截取一次，按16进制解析
                    bytes[i] = System.Convert.ToByte(cleanHex.Substring(i * 2, 2), 16);
                }

                return bytes;
            }
            catch
            {
                // 解析失败（例如包含非Hex字符 G, H 等），返回 null 或 DependencyProperty.UnsetValue
                return null;
            }
        }
    }
}