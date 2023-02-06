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
    }
}
