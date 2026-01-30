using System;

namespace WorkOrderApplication.API.Extensions;

public static class DateTimeExtensions
{
    // แปลงจาก UTC(เวลามาตรฐานสากล) → ICT(เวลาในประเทศไทย)
        // ✅ เก็บ TimeZone ไว้ใน static field เพื่อไม่ต้องเรียก FindSystemTimeZoneById บ่อย ๆ
        private static readonly TimeZoneInfo ThaiZone =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        /// <summary>
        /// แปลงจาก UTC → ICT (เวลาไทย)
        /// </summary>
        public static DateTime ToICT(this DateTime utcDateTime)
        {
            // ✅ ป้องกันกรณีที่ DateTime ไม่ได้ถูกระบุว่าเป็น UTC
            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, ThaiZone);
        }

        /// <summary>
        /// แปลงจาก UTC → ICT (รองรับ DateTime? nullable)
        /// </summary>
        public static DateTime? ToICT(this DateTime? utcDateTime)
        {
            return utcDateTime.HasValue
                ? utcDateTime.Value.ToICT()
                : null;
        }
}