using System;
using System.IO;
using System.Net;

namespace PCL2.Neo.Models.Account.OAuthService.RedirectServer;

public class ResponseHelper(HttpListenerResponse response)
{
    private readonly HttpListenerResponse _response = response;
    private readonly Stream _outputStream = response.OutputStream;

    public record FileObject
    {
        public FileStream FileStream;
        public byte[] Buffer;
    }

    public void WriteContent(FileStream fileStream)
    {
        _response.StatusCode = 200;
        var buffer = new byte[1024];
        var obj = new FileObject() { FileStream = fileStream, Buffer = buffer };
        fileStream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(EndWirte), obj);
    }

    private void EndWirte(IAsyncResult asyncResult)
    {
        var obj = asyncResult.AsyncState as FileObject;
        var number = obj.FileStream.EndRead(asyncResult);
        _outputStream.Write(obj.Buffer, 0, number);
        if (number < 1)
        {
            obj.FileStream.Close();
            _outputStream.Close();
            return;
        }

        obj.FileStream.BeginRead(obj.Buffer, 0, obj.Buffer.Length, new AsyncCallback(EndWirte), obj);
    }
}