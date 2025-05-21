using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aurora.Server.Communication
{
    public static class Constants
    {
        public const string PasswordPattern = @"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!@$#%^&*]).{8,32}$";
        public const string EmailPattern = @"^[\w\.-]+@([\w-]+\.)+[\w-]{2,4}$";
        public const string AddressPattern = @"^([A-Za-z\s]+),\s(\d+),\s([A-Za-z]+)$";
        public const string PhonePattern = @"^0\d{1,2}-\d{7}$";
        public const string BirthdayPattern = @"^(0[1-9]|[1-2][0-9]|3[0-1])\.(0[1-9]|1[0-2])\.\d{4}$";
        public const string QuestionMarkPattern = @"\?$";

        public static readonly Regex PasswordRegex = new Regex(PasswordPattern, RegexOptions.Compiled);
        public static readonly Regex EmailRegex = new Regex(EmailPattern, RegexOptions.Compiled);
        public static readonly Regex AddressRegex = new Regex(AddressPattern, RegexOptions.Compiled);
        public static readonly Regex PhoneRegex = new Regex(PhonePattern, RegexOptions.Compiled);
        public static readonly Regex BirthdayRegex = new Regex(BirthdayPattern, RegexOptions.Compiled);
        public static readonly Regex QuestionMarkRegex = new Regex(QuestionMarkPattern, RegexOptions.Compiled);
    }
}
