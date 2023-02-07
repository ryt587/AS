namespace _211933M_Assn.Services
{
    public class EncodingService
    {
        public class Encoding
        {
            //Encoding
        }
        public static string EncodingMethod(string Data)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.UTF8.GetBytes(Data);
            string sReturnValues = System.Convert.ToBase64String(toEncodeAsBytes);
            return sReturnValues;
        }
        //Decoding
        public static string DecodingMethod(string Data)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(Data);
            string returnValue = System.Text.Encoding.UTF8.GetString(encodedDataAsBytes);
            return returnValue;
        }
        public static string EncodingEmail(string Data)
        {
            var gmail = Data.Split("@")[0];
            var provider = Data.Split("@")[1];
            byte[] toEncodeAsBytes = System.Text.Encoding.UTF8.GetBytes(gmail);
            string sReturnValues = System.Convert.ToBase64String(toEncodeAsBytes)+"@"+provider;
            return sReturnValues;
        }
        public static string DecodingEmail(string Data)
        {
            var gmail = Data.Split("@")[0];
            var provider = Data.Split("@")[1];
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(gmail);
            string returnValue = System.Text.Encoding.UTF8.GetString(encodedDataAsBytes) +"@"+provider;
            return returnValue;
        }
    }
}
