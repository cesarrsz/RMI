using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

[Serializable]
public class RemoteRequest
{
    public string MethodName { get; set; }
    public object[] Parameters { get; set; }
}

[Serializable]
public class RemoteResponse
{
    public object Result { get; set; }
    public string ErrorMessage { get; set; }
}

class RmiSimulation
{
    static void Main(string[] args)
    {
        Console.WriteLine("Selecciona el modo: ");
        Console.WriteLine("1. Servidor");
        Console.WriteLine("2. Cliente");
        int option = int.Parse(Console.ReadLine());

        if (option == 1)
        {
            RunServer();
        }
        else if (option == 2)
        {
            RunClient();
        }
        else
        {
            Console.WriteLine("Opción inválida.");
        }
    }

    static void RunServer()
    {
        int port = 8080;
        TcpListener server = null;

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Servidor iniciado en el puerto {port}.");

            while (true)
            {
                Console.WriteLine("Esperando conexión...");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Cliente conectado.");

                NetworkStream stream = client.GetStream();
                BinaryFormatter formatter = new BinaryFormatter();

                // Recibir solicitud
                RemoteRequest request = (RemoteRequest)formatter.Deserialize(stream);
                Console.WriteLine($"Método solicitado: {request.MethodName}");

                // Procesar solicitud
                RemoteResponse response = ProcessRequest(request);

                // Enviar respuesta
                formatter.Serialize(stream, response);
                client.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            server?.Stop();
        }
    }

    static RemoteResponse ProcessRequest(RemoteRequest request)
    {
        try
        {
            switch (request.MethodName)
            {
                case "GetMessage":
                    string name = (string)request.Parameters[0];
                    return new RemoteResponse { Result = $"Hola, {name}! Bienvenido al servidor simulado." };

                case "AddNumbers":
                    int a = (int)request.Parameters[0];
                    int b = (int)request.Parameters[1];
                    return new RemoteResponse { Result = a + b };

                default:
                    return new RemoteResponse { ErrorMessage = "Método no reconocido." };
            }
        }
        catch (Exception ex)
        {
            return new RemoteResponse { ErrorMessage = ex.Message };
        }
    }

    static void RunClient()
    {
        string serverAddress = "127.0.0.1";
        int port = 8080;

        try
        {
            using (TcpClient client = new TcpClient(serverAddress, port))
            {
                Console.WriteLine("Conectado al servidor.");

                NetworkStream stream = client.GetStream();
                BinaryFormatter formatter = new BinaryFormatter();

                // Solicitar "GetMessage"
                RemoteRequest request = new RemoteRequest
                {
                    MethodName = "GetMessage",
                    Parameters = new object[] { "Juan" }
                };
                formatter.Serialize(stream, request);

                RemoteResponse response = (RemoteResponse)formatter.Deserialize(stream);
                if (response.ErrorMessage == null)
                {
                    Console.WriteLine($"Respuesta del servidor: {response.Result}");
                }
                else
                {
                    Console.WriteLine($"Error del servidor: {response.ErrorMessage}");
                }

                // Solicitar "AddNumbers"
                request = new RemoteRequest
                {
                    MethodName = "AddNumbers",
                    Parameters = new object[] { 5, 7 }
                };
                formatter.Serialize(stream, request);

                response = (RemoteResponse)formatter.Deserialize(stream);
                if (response.ErrorMessage == null)
                {
                    Console.WriteLine($"Respuesta del servidor: {response.Result}");
                }
                else
                {
                    Console.WriteLine($"Error del servidor: {response.ErrorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
