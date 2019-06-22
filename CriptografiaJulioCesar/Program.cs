using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace CriptografiaJulioCesar
{
    class Program
    {
        //SNIPE 208
        static void Main(string[] args)
        {
            #region REQUISIÇÃO

            //var requisicaoWeb = WebRequest.CreateHttp("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=91e93d89f007589a868fabc78a7354db1b3564d0");
            //requisicaoWeb.Method = "GET";
            //requisicaoWeb.UserAgent = "RequisicaoWebDemo";

            //using (var resposta = requisicaoWeb.GetResponse())
            //{
            //    var streamDados = resposta.GetResponseStream();
            //    StreamReader reader = new StreamReader(streamDados);
            //    object objResponse = reader.ReadToEnd();
            //    //Console.WriteLine(objResponse.ToString());
            //    ConverterJsonParaObjeto(objResponse.ToString());
            //    Console.ReadLine();
            //    streamDados.Close();
            //    resposta.Close();
            //}

            #endregion

            string json_challenge = "{" +
              " 'numero_casas':2, " +
              " 'token':'91e93d89f007589a868fabc78a7354db1b3564d0'," +
              " 'cifrado':'lwuv fq pqv etgcvg c hkng ecnngf -th. nctta ycnn'," +
              " 'decifrado':''," +
              " 'resumo_criptografico':'' " +
              "}";

            Json jsonModel = JsonConvert.DeserializeObject<Json>(json_challenge);
            Descriptografa(jsonModel);
            ConverterObjetoParaJson(jsonModel);
            Console.WriteLine($"{jsonModel.numero_casas}\n{jsonModel.token}\n{jsonModel.cifrado}\n{jsonModel.decifrado}\n{jsonModel.resumo_criptografico}");

            //VERIFICAR SE ESTÁ PARECIDADA COM EXEMPLO
            string teste_codenation = "a ligeira raposa marrom saltou sobre o cachorro cansado 1a.a";
            Criptografa(teste_codenation,3);

            Console.ReadKey();

        }

        #region CONVERSÃO JSON

        //static void ConverterJsonParaObjeto()
        //{
        //}
        static void ConverterObjetoParaJson(Json json_Model)
        {
            Json json_topico = new Json()
            {
                numero_casas = json_Model.numero_casas,
                token = json_Model.token,
                cifrado = json_Model.cifrado,
                decifrado = json_Model.decifrado,
                resumo_criptografico = json_Model.resumo_criptografico,
            };

            string json = JsonConvert.SerializeObject(json_topico);

            Post(json);

            //Console.WriteLine(json);
        }

        #endregion

        static void Criptografa(string mensagem,int cifra)
        {
            //CRIPTOGRAFIA

            string crypt = "";
            foreach (char c in mensagem)
            {
                if (char.IsNumber(c) || char.IsPunctuation(c))
                {
                    crypt += (char)(c);
                }
                else
                {
                    crypt += (char)(c + cifra);
                }
            }
            crypt = crypt.Replace("#", " ").ToLower();
            Console.WriteLine(crypt);
            DescriptografaString(crypt, 3);


        }

        static void Descriptografa(Json Model)
        {
            //DESCRIPTOGRAFIA
            //Console.Write("Palavra Descriptografada: ");
            int cifra = Model.numero_casas;
            string des_crypt = "";
            foreach (char c in Model.cifrado)
            {
                if (char.IsNumber(c) || char.IsPunctuation(c))
                {
                    des_crypt += (char)(c);
                }
                else
                {
                    des_crypt += (char)(c - cifra);
                }
            }
            des_crypt = des_crypt.Replace("\u001e", " ");
            Model.decifrado = des_crypt.ToLower();

            CalculateSHA1(Model);


            //Console.WriteLine();

            //Console.WriteLine(des_crypt);

            //ConverterObjetoParaJson(crypt, des_crypt);
            //ConverterJsonParaObjeto(crypt, des_crypt);

            //Console.ReadLine();
        }

        //VERIFICAR SE ESTÁ PARECIDADA COM EXEMPLO
        static void DescriptografaString(string mensagem, int cifra)
        {
            string des_crypt = "";
            foreach (char c in mensagem)
            {
                if (char.IsNumber(c) || char.IsPunctuation(c))
                {
                    des_crypt += (char)(c);
                }
                else
                {
                    des_crypt += (char)(c - cifra);
                }
            }
            des_crypt = des_crypt.Replace("\u001d", " ");
            Console.WriteLine(des_crypt);
            //Console.WriteLine();

            //Console.WriteLine(des_crypt);

            //ConverterObjetoParaJson(crypt, des_crypt);
            //ConverterJsonParaObjeto(crypt, des_crypt);

            //Console.ReadLine();
        }

        static void  CalculateSHA1(Json Resumo)
        {
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(Resumo.cifrado);
                System.Security.Cryptography.SHA1CryptoServiceProvider cryptoTransformSHA1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                Resumo.resumo_criptografico = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }
        }
        public  static string Post(string value)
        {
            var request = HttpWebRequest.Create("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=91e93d89f007589a868fabc78a7354db1b3564d0");
            var byteData = Encoding.ASCII.GetBytes(value);
            request.ContentType = "multipart/form-data";
            request.ContentType = "application/json";
            request.Method = "POST";

            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteData, 0, byteData.Length);
                }
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return responseString;
            }
            catch (WebException e)
            {
                return null;
            }
        }
    }

}


