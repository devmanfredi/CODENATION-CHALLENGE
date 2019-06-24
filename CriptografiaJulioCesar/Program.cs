﻿using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Mime;
using System.Collections.Generic;

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
            Enviar();

        }

        /// <summary>
        /// FAZ A REQUISIÇÃO E SALVA O ARQUIVO JSON
        /// </summary>
        static void Requisição()
        {
            var requisicaoWeb = WebRequest.CreateHttp("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=91e93d89f007589a868fabc78a7354db1b3564d0");
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

            Console.ReadKey();


            stream.Close();
        }

        //AQUI COMEÇA O PROCESSO PARA ENVIAR O ARQUIVO

        static void Enviar()
        {
            WebRequest request = WebRequest.Create("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=91e93d89f007589a868fabc78a7354db1b3564d0");
            request.Method = "POST";
            byte[] byteArray = File.ReadAllBytes("C:\\CodeNationChallenge\\answer.json");
            request.ContentType = "multipart/form-data";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            //reader.Close();
            dataStream.Close();
            response.Close();
        }

        static async void CreatePerson(Json Model)
        {

            // Cria a pessoa e armazena o resultado
            var isOK = await CreatePersonAsync(Model);

            // Verifica se obteve sucesso
            if (isOK)
            {
                // Pessoa criada com sucesso
            }
        }

        /// <summary>
        /// Cria uma pessoa
        /// </summary>
        /// <param name="person">Objeto 'Person'</param>
        /// <returns></returns>
        static async Task<bool> CreatePersonAsync(Json Model)
        {

            //File answer = "answer.json";

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=91e93d89f007589a868fabc78a7354db1b3564d0");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Transforma o objeto em json
            string json = JsonConvert.SerializeObject(Model);

            // Envia o json para a API e verifica se obteve sucesso
            HttpResponseMessage response = await client.PostAsync("CreatePerson", new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }

    }


}


