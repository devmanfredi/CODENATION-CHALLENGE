using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace CriptografiaJulioCesar
{
    class Program
    {
        //SNIPE 208
        static void Main(string[] args)
        {
            //Requisição();
            Json challenge = LerObjetoDeArquivo();
            Descriptografa(challenge);
            ResumoSHA1(challenge);
            SalvarObjetoEmArquivo(challenge);
            var ImprimeScore = Enviar();
            Console.WriteLine(ImprimeScore.ToString());

            Console.ReadKey();

        }

        /// <summary>
        /// FAZ A REQUISIÇÃO E SALVA O ARQUIVO JSON
        /// </summary>
        static void Requisição()
        {
            var requisicaoWeb = WebRequest.CreateHttp("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=xxxxxxxxxxxxxxxxxxxxxxx");
            requisicaoWeb.Method = "GET";
            requisicaoWeb.UserAgent = "RequisicaoWebDemo";

            using (var resposta = requisicaoWeb.GetResponse())
            {
                var streamDados = resposta.GetResponseStream();
                StreamReader reader = new StreamReader(streamDados);
                object objResponse = reader.ReadToEnd();

                //RETORNA UM MODELO PADRAO PARA SALVAR EM JSON
                object new_objResponse =  ConverteJsonParaObjeto(objResponse.ToString());
                
                //SALVA A REQUISIÇÃO EM UM ARQUIVO
                StreamWriter stream = new StreamWriter("C:\\CodeNationChallenge\\answer.json");
                JsonTextWriter writer = new JsonTextWriter(stream);
                JsonSerializer serializer = new JsonSerializer();
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, new_objResponse);

                stream.Close();

                streamDados.Close();
                resposta.Close();

            }
        }
        
        /// <summary>
        /// PEGA A REQUISIÇÃO E PASSA PARA UM MODEL(OBJETO)
        /// </summary>
        /// <param name="objResponse"></param>
        /// <returns></returns>
        static Json ConverteJsonParaObjeto(string objResponse)
        {
            Json Model = JsonConvert.DeserializeObject<Json>(objResponse);
            return Model;
        }

        /// <summary>
        /// LER O ARQUIVO DA REQUISIÇÃO E RETORNA UM OBJETO PARA  DESCRIPTOGRAFAR
        /// </summary>
        /// <returns></returns>
        static Json LerObjetoDeArquivo()
        {
            StreamReader stream = new StreamReader("C:\\CodeNationChallenge\\answer.json");
            JsonTextReader reader = new JsonTextReader(stream);
            JsonSerializer serializer = new JsonSerializer();

            Json Model = serializer.Deserialize<Json>(reader);

            stream.Close();

            return Model;

        }

        /// <summary>
        /// RECEBE O MODEL E DESCRIPTOGRAFA O CAMPO CIFRADO
        /// </summary>
        /// <param name="Model"></param>
        static void Descriptografa(Json Model)
        {
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
                    if (c == 'a')
                    {
                        des_crypt += 'y';
                    }
                    else
                    {
                        des_crypt += (char)((int)(c - cifra));
                    }
                }
            }

            des_crypt = des_crypt.Replace("\u001e", " ");
            Model.decifrado = des_crypt.ToLower();

        }

        /// <summary>
        /// RECEBE O MODEL JÁ DESCRIPTOGRAFADO PARA FAZER O RESUMO CRIPTOGRÁFICO
        /// </summary>
        /// <param name="Resumo"></param>
        static Json ResumoSHA1(Json Model)
        {
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(Model.decifrado);
                System.Security.Cryptography.SHA1CryptoServiceProvider cryptoTransformSHA1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                Model.resumo_criptografico = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).ToLower().Replace("-","");
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }

            return Model;
        }

        /// <summary>
        /// AQUI VAI SALVAR O ARQUIVO JSON COMPLETO
        /// </summary>
        /// <param name="answer"></param>
        static void SalvarObjetoEmArquivo(Json Model)
        {
            StreamWriter stream = new StreamWriter("C:\\CodeNationChallenge\\answer.json");
            JsonTextWriter writer = new JsonTextWriter(stream);
            JsonSerializer serializer = new JsonSerializer();

            writer.Formatting = Formatting.Indented;

            serializer.Serialize(writer, Model);

            //PARA VER SE ESTÁ FORMATADO E COMPLETO.
            var json = JsonConvert.SerializeObject(Model);
            var _json = JValue.Parse(json).ToString(Formatting.Indented);

            Console.WriteLine(_json);

            //Console.ReadKey();


            stream.Close();
        }

        /// <summary>
        /// ENVIA O ARQUIVO PARA A API E RETORNA O SCORE
        /// </summary>
        /// <returns></returns>
        static object Enviar()
        {
            Json Model = LerObjetoDeArquivo();

            string json = JsonConvert.SerializeObject(Model);
            var url = "https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            boundary = "--" + boundary;

            using (var stream = request.GetRequestStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(boundary);
                writer.WriteLine("Content-Disposition: form-data; name=\"answer\"; filename=\"C:\\CodeNationChallenge\\answer.json\"");
                writer.WriteLine("Content-Type: application/json");
                writer.WriteLine();
                writer.Write(json);
                writer.WriteLine();
                writer.WriteLine(boundary + "--");
            }

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            { string strResponse = reader.ReadToEnd(); return JObject.Parse(strResponse); }
        }
    }
}


