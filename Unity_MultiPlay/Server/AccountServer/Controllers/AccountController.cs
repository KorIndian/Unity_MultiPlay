using AccountServer.DB;
using AccountServer.Http;
using CommonWebPacket;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AccountServer.Controllers
{
	[Route("api/[controller]")] // => 실제 루트는 "api/Account" 이렇게된다. 
	[ApiController]
	public class AccountController : ControllerBase
	{
		AppDbContext context;
		

		public AccountController(AppDbContext _context)
		{
			context = _context;
		}

		[HttpPost]
		[Route("create")]
		public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
		{
			CreateAccountPacketRes res = new CreateAccountPacketRes();

			AccountDb? account = context.Accounts
				.AsNoTracking()
				.Where(a => a.AccountName == req.AccountName)//Name에 인덱싱을 걸어 두었기 때문에 탐색이 빠르다.
				.FirstOrDefault();

			if(account ==null)
			{
				context.Accounts.Add(new AccountDb()
				{
					AccountName = req.AccountName,
					Password = req.Password
				});

				bool success = context.SaveChangesEx();
				res.CreateOk = success;
			}
			else
			{
				res.CreateOk = false;
			}

			return res;
		}

		

		[HttpPost]
		[Route("login")]
		public LoginAccountPacketRes LoginAccount([FromBody] LoginAccountPacketReq req)
		{
			LoginAccountPacketRes res = new LoginAccountPacketRes();

			AccountDb? account = context.Accounts
				.AsNoTracking()
				.Where(a => a.AccountName == req.AccountName && a.Password == req.Password)
				.FirstOrDefault();

			if (account == null)
			{
				res.LoginOk = false;
			}
			else
			{
				res.LoginOk = true;
				//일단 가라로 만든다.
				res.ServerList = new List<ServerStatus>()
				{
					new ServerStatus() { Name = "Korea", Ip = "127.0.0.1", CrowdedLevel = 0 },
					new ServerStatus() { Name = "America", Ip = "127.0.0.1", CrowdedLevel = 3 }
				};
				QueryServerStatusReq queryServerStatus = new QueryServerStatusReq() { data = "test" };

				AccountServerHttpClient.HttpSendObject<QueryServerStatusRes>(queryServerStatus, HttpMethod.Post, nameof(QueryServerStatusReq));
			}

			return res;
		}
	}
}
