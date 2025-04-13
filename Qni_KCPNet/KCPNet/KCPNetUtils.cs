// ------------------------------------
//
// FileName: KCPNetServer.cs
//
// Author:   lisonghappy
// Email:    lisonghappy@gmail.com
// Date:     2025/4/12
// Desc:     Net Utils
//
// ------------------------------------


using System;
using System.Text;
using System.Threading;

namespace KCPNet
{
    public class KCPNetUtils {

        /// <summary>
        /// Log utils
        /// </summary>
        public static class Logger {

            public enum ELogColor {
                None,
                Red,
                Green,
                Blue,
                Cyan,
                Magenta,
                Yellow,
                White,
                Gray,
                Black
            }

            public static Action<string> LogFunc;
            public static Action<string> LogWarningFunc;
            public static Action<string> LogErrorFunc;
            public static Action<ELogColor, string> LogWithColorFunc;

            #region --------------------- Log ---------------------
            public static void Log (object msg) {
                var _msg = msg.ToString();
                if (LogFunc != null) {
                    LogFunc(_msg);
                }
                else {
                    OutputLogInfo(ELogColor.None, _msg);
                }
            }

            public static void LogFormat (string format, params object[] args) {
                var _msg = string.Format(format, args);
                if (LogFunc != null) {
                    LogFunc(_msg);
                }
                else {
                    OutputLogInfo(ELogColor.None, _msg);
                }
            }
            #endregion

            #region --------------------- LogWarning ---------------------

            public static void LogWarning (object msg) {
                var _msg = msg.ToString();
                if (LogWarningFunc != null) {
                    LogWarningFunc(_msg);
                }
                else {
                    OutputLogInfo(ELogColor.Yellow, _msg);
                }
            }

            public static void LogWarningFormat (string format, params object[] args) {
                var _msg = string.Format(format, args);
                if (LogWarningFunc != null) {
                    LogWarningFunc(_msg);
                }
                else {
                    OutputLogInfo(ELogColor.Yellow, _msg);
                }
            }
            #endregion

            #region --------------------- LogError ---------------------
            public static void LogError (object msg) {
                var _msg = msg.ToString();
                if (LogErrorFunc != null) {
                    LogErrorFunc(_msg);
                }
                else {
                    OutputLogInfo(ELogColor.Red, _msg);
                }
            }
            public static void LogErrorFormat (string format, params object[] args) {
                var _msg = string.Format(format, args);
                if (LogErrorFunc != null) {
                    LogErrorFunc(_msg);
                }
                else {
                    OutputLogInfo(ELogColor.Red, _msg);
                }
            }
            #endregion

            #region --------------------- LogColor ---------------------

            public static void LogWithColor (ELogColor color, object msg) {
                var _msg = msg.ToString();
                if (LogWithColorFunc != null) {
                    LogWithColorFunc(color, _msg);
                }
                else {
                    OutputLogInfo(color, _msg);
                }
            }

            public static void LogWithColorFormat (ELogColor color, string format, params object[] args) {
                var _msg = string.Format(format, args);
                if (LogWithColorFunc != null) {
                    LogWithColorFunc(color, _msg);
                }
                else {
                    OutputLogInfo(color, _msg);
                }
            }
            #endregion


            private static void OutputLogInfo (ELogColor color, string msg) {
                StringBuilder sb = new StringBuilder();
                TimeZoneInfo currentTimeZone = TimeZoneInfo.Local;
                sb.AppendFormat(" [{0}(UTC,{1},{2})]", DateTime.UtcNow.ToString("yyyy.MM.dd.HH:mm:ss.fff"), currentTimeZone.BaseUtcOffset, currentTimeZone.Id);
                sb.AppendFormat(" ThreadID:{0} >", Thread.CurrentThread.ManagedThreadId);
                sb.Append(msg);

                var _beforeColor = Console.ForegroundColor;
                var _newColor = _beforeColor;

                switch (color) {
                    case ELogColor.Red:
                        _newColor = ConsoleColor.DarkRed;
                        break;
                    case ELogColor.Green:
                        _newColor = ConsoleColor.Green;
                        break;
                    case ELogColor.Blue:
                        _newColor = ConsoleColor.Blue;
                        break;
                    case ELogColor.Cyan:
                        _newColor = ConsoleColor.Cyan;
                        break;
                    case ELogColor.Magenta:
                        _newColor = ConsoleColor.Magenta;
                        break;
                    case ELogColor.Yellow:
                        _newColor = ConsoleColor.DarkYellow;
                        break;
                    case ELogColor.White:
                        _newColor = ConsoleColor.White;
                        break;
                    case ELogColor.Gray:
                        _newColor = ConsoleColor.Gray;
                        break;
                    case ELogColor.Black:
                        _newColor = ConsoleColor.Black;
                        break;
                    case ELogColor.None:
                    default:
                        break;
                }


                Console.ForegroundColor = _newColor;
                Console.WriteLine(sb.ToString());
                Console.ForegroundColor = _beforeColor;
            }
        }


        private static readonly DateTime UTCStartDate = new DateTime (1970,1,1,0,0,0);
        /// <summary>
        /// <para> 获得从 [UTC] 开始时间(1970.1.1)到 [当前时间] 的毫秒差值。</para>
        /// <para> Get the millisecond difference from [UTC] start time(1970.1.1) to [UtcNow].</para>
        /// </summary>
        /// <returns></returns>
        public static ulong GetMillisecondsFromUTCStartDate () {
            TimeSpan timeSpan = DateTime.UtcNow - UTCStartDate;
            return(ulong)timeSpan.TotalMilliseconds;
        }
    }
}

