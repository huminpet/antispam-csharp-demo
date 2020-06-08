using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class KeywordSubmitApiDemo
    {
        public static void keywordSubmit()
        {     
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 易盾反垃圾云服务敏感词提交接口地址  */
            String apiUrl = "http://as.dun.163.com/v1/keyword/submit";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v1");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            // 100: 色情，110: 性感，200: 广告，210: 二维码，300: 暴恐，400: 违禁，500: 涉政，600: 谩骂，700: 灌水
            parameters.Add("category", "100");
            List<string> keywords = new List<string>();
            keywords.Add("色情敏感词1");
            keywords.Add("色情敏感词2");
            parameters.Add("keywords", string.Join(",",keywords.ToArray()));

            // 3.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 4.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 1000);
            if(result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    // 敏感词添加成功结果数组
                    JArray resultArray = (JArray)ret.SelectToken("result");
                }
                else
                {
                    Console.WriteLine(String.Format("ERROR: code={0}, msg={1}", code, msg));
                }
            }
            else
            {
                Console.WriteLine("Request failed!");
            }

        }      
    }
}
