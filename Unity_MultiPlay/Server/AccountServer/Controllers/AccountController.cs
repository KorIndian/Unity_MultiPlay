using AccountServer.DB;
using AccountServer.Http;
using CommonWebPacket;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SharedDB;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static SharedDB.DataModel;

namespace AccountServer.Controllers
{
	[Route("api/[controller]")] // => 실제 루트는 "api/Account" 이렇게된다. 
	[ApiController]
	public class AccountController : ControllerBase
	{
		AppDbContext appDbContext;
		SharedDbContext sharedDbContext;

		//Db가 여러개여도 파라미터를 늘려주면 알아서 인젝션이 된다..
		public AccountController(AppDbContext _context, SharedDbContext _sharedDbContext)
		{
			appDbContext = _context;
			sharedDbContext = _sharedDbContext;
		}

		[HttpPost]
		[Route("create")]
		public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
		{
			CreateAccountPacketRes res = new CreateAccountPacketRes();

			AccountDb? account = appDbContext.Accounts
				.AsNoTracking()
				.Where(a => a.AccountName == req.AccountName)//Name에 인덱싱을 걸어 두었기 때문에 탐색이 빠르다.
				.FirstOrDefault();

			if(account == null)
			{
				appDbContext.Accounts.Add(new AccountDb()
				{
					AccountName = req.AccountName,
					Password = req.Password
				});

				bool success = appDbContext.SaveChangesEx();
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

			AccountDb? account = appDbContext.Accounts
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

				//토큰 발급
				DateTime expired = DateTime.UtcNow;
				expired.AddSeconds(600);

				TokenDb? tokenDb = sharedDbContext.Tokens
					.Where(t =>t.AccountDbId == account.AccountDbId)
					.FirstOrDefault();

				if(tokenDb != null)
				{
					tokenDb.Token = new Random().Next(int.MinValue, int.MaxValue);
					tokenDb.Expired = expired;
					sharedDbContext.SaveChangesEx();
				}
				else
				{
					tokenDb = new TokenDb()
					{
						AccountDbId = account.AccountDbId,
						AccountName = account.AccountName,
						Token = new Random().Next(int.MinValue, int.MaxValue),
						Expired = expired
					};
					sharedDbContext.Add(tokenDb);
					sharedDbContext.SaveChangesEx();
				}

				res.AccountName = account.AccountName;
				res.AccountId = account.AccountDbId;
				res.Token = tokenDb.Token;

				foreach (var serverInfo in sharedDbContext.Servers)
				{
					res.ServerList.Add(new ServerStatus() 
					{ 
						Name = serverInfo.Name,
						IpAddress = serverInfo.IpAddress,
						Port = serverInfo.Port,
						CrowdedLevel = serverInfo.CrowdedLevel
					});
				}

				//Test Query
				//QueryServerStatusReq queryServerStatus = new QueryServerStatusReq() { data = "test" };
				//AccountServerHttpClient.HttpSendObject<QueryServerStatusRes>(queryServerStatus, HttpMethod.Post, nameof(QueryServerStatusReq));
			}

			return res;
		}
	}
}
