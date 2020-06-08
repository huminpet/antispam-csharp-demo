using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class AudioCallbackApiDemo
    {
        public static void audioCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务音频离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163.com/v3/audio/callback/results";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v3.1");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 3.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 10000);
            if(result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    JArray array = (JArray)ret.SelectToken("result");
                    foreach (var item in array)
                    {
                        JObject jObject = (JObject)item;
                        String taskId = jObject.GetValue("taskId").ToObject<String>();
                        int asrStatus = jObject.GetValue("asrStatus").ToObject<Int32>();
                        if (asrStatus == 4)
                        {
                            int asrResult = jObject.GetValue("asrResult").ToObject<Int32>();
                            Console.WriteLine(String.Format("检测失败: taskId={0}, asrResult={1}", taskId, asrResult));
                        }
                        else
                        {
                            int action = jObject.GetValue("action").ToObject<Int32>();
                            JArray labelArray = (JArray)jObject.SelectToken("labels");
                            if (action == 0) {
                                Console.WriteLine(String.Format("结果：通过!taskId={0}", taskId));
                            } else if (action == 2 || action == 1) {
                                /*foreach  (var labelElement in labelArray)
                                {
                                    JObject lObject = (JObject)labelElement;
                                    int label = lObject.GetValue("label").ToObject<Int32>();
                                    int level = lObject.GetValue("level").ToObject<Int32>();
                                    JObject detailsObject = (JObject)lObject.SelectToken("details");
                                    JArray hintArray = (JArray)detailsObject.SelectToken("hint");
                                    // 二级细分类
                                    JArray subLabels = (JArray)detailsObject.SelectToken("subLabels");
                                }*/
                                Console.WriteLine(String.Format("结果：{0}!taskId={1}", action == 1 ? "不确定" : "不通过",taskId));
                            }
                        }
                    }
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
