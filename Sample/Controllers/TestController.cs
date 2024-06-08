using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Application.ExceptionHandling;
using Jarvis.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    [HttpGet("file/text")]
    public IActionResult FileText()
    {
        var bytes = Encoding.UTF8.GetBytes("hahaha");
        return new FileContentResult(bytes, ContentType.Text);
    }

    [HttpGet("file/xml")]
    public IActionResult FileXml()
    {
        var bytes = Encoding.UTF8.GetBytes("<tag1>hahaha</tag1>");
        return new FileContentResult(bytes, ContentType.Xml);
    }

    [HttpGet("content/xml/not-wrap")]
    public IActionResult ContentNotWrapXml()
    {
        return Ok("<tag1>hahaha</tag1>");
    }

    [HttpGet("content/json/not-wrap")]
    public IActionResult ContentNotWrapJson()
    {
        return Ok(new
        {
            Field1 = "haha",
            Field2 = "hehe"
        });
    }

    [HttpGet("content/json/wrap-json")]
    public IActionResult ContentJsonWrapJson()
    {
        return Ok(new
        {
            Field1 = "haha",
            Field2 = "hehe"
        });
    }

    [HttpGet("content/text")]
    public IActionResult String()
    {
        return Ok("hahaha");
    }

    [HttpGet("content/boolean")]
    public IActionResult Boolean()
    {
        return Ok(true);
    }

    [HttpGet("exception")]
    public IActionResult Exception()
    {
        throw new BusinessException(9999, "Something when wrong");
    }
}