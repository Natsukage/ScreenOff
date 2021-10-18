using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;

namespace ScreenOff
{
    public delegate void ReceivedRequestEventHandler(string payload);
    public delegate void OnExceptionEventHandler(Exception ex);

    class HttpServer
    {
        private Thread _serverThread;
        private HttpListener _listener;
        public int Port { get; private set; }

        public event ReceivedRequestEventHandler ReceivedCommandRequest;
        public event OnExceptionEventHandler OnException;

        /// <summary>
        ///     在指定端口启动监听
        /// </summary>
        /// <param name="port">要启动的端口</param>
        public HttpServer(int port)
        {
            Initialize(port);
        }

        /// <summary>
        ///     停止监听并释放资源
        /// </summary>
        public void Stop()
        {
            //_serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://*:" + Port + "/");
                _listener.Start();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
                return;
            }

            ThreadPool.QueueUserWorkItem(o => {
                try
                {
                    while (_listener.IsListening)
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            if (c is not HttpListenerContext context)
                                throw new ArgumentNullException(nameof(context));
                            try
                            {
                                DoAction(ref context);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }
                            finally
                            {
                                context.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                }
                catch
                {
                    // ignored
                }
            });
        }

        private void DoAction(ref HttpListenerContext context)
        {

            var command = new StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEnd();

            ReceivedCommandRequest?.Invoke(command);

            var buf = Encoding.UTF8.GetBytes(command);
            context.Response.ContentLength64 = buf.Length;
            context.Response.OutputStream.Write(buf, 0, buf.Length);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.OutputStream.Flush();
        }

        private void Initialize(int port)
        {
            Port = port;
            _serverThread = new Thread(Listen);
            _serverThread.Start();
        }

    }
}
