using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using ParallelEfContext.Context;
using ParallelEfContext.Model;
using System.Diagnostics.Metrics;
using System.Threading;

namespace ParallelEfContext.Controllers
{
    [ApiController]
    public class LetterController : ControllerBase
    {
        private readonly ILogger<LetterController> _logger;
        private readonly AppDbContext _appDbContext;
        private readonly IDbContextPool<AppDbContext> _dbContextPool;

        public LetterController(ILogger<LetterController> logger, AppDbContext appDbContext, IDbContextPool<AppDbContext> dbContextPool)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _dbContextPool = dbContextPool;
        }

        [HttpGet("/letter")]
        public async Task<IActionResult> Get()
        {
            return Ok(_appDbContext.Letters);
        }

        [HttpPost("/sequentional")]
        public async Task<IActionResult> Sequentional(char[] letters)
        {
            var counter = 0;
            letters.Select(async x =>
            {
                await _appDbContext.AddAsync(new Letter
                {
                    Char = x
                });

                await _appDbContext.SaveChangesAsync();
                _logger.LogInformation(string.Format("[Thread {0}]: Current elements -> {1}", counter++, string.Join(", ", _appDbContext.Letters.Select(x => x.Char))));
            }).ToList();

            return Ok("Done");
        }

        [HttpPost("/bad-parallel")]
        public async Task<IActionResult> BadParallel(char[] letters)
        {
            try
            {
                await Parallel.ForEachAsync(letters, async (letter, cancellationToken) =>
                {
                    var threadId = Guid.NewGuid();

                    await _appDbContext.AddAsync(new Letter
                    {
                        Char = letter
                    }, cancellationToken);

                    await _appDbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation(string.Format("[Thread {0}]: Current elements -> {1}", threadId, string.Join(", ", _appDbContext.Letters.Select(x => x.Char))));
                });
            } catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return Ok("Done");
        }

        [HttpPost("/mutex")]
        public async Task<IActionResult> Mutex(char[] letters)
        {
            var mutex = new Mutex();
            await Parallel.ForEachAsync(letters, async (letter, cancellationToken) =>
            {
                var threadId = Guid.NewGuid();

                mutex.WaitOne();
                await _appDbContext.AddAsync(new Letter
                {
                    Char = letter
                }, cancellationToken);

                await _appDbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(string.Format("[Thread {0}]: Current elements -> {1}", threadId, string.Join(", ", _appDbContext.Letters.Select(x => x.Char))));
                mutex.ReleaseMutex();
            });
            return Ok("Done");
        }

        [HttpPost("/new-connection")]
        public async Task<IActionResult> NewConnection(char[] letters)
        {
            await Parallel.ForEachAsync(letters, async (letter, cancellationToken) =>
            {
                var threadId = Guid.NewGuid();
                var currentContext = new AppDbContextFactory().CreateDbContext(new string[] { });

                await currentContext.AddAsync(new Letter
                {
                    Char = letter
                }, cancellationToken);

                await currentContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(string.Format("[Thread {0}]: Current elements -> {1}", threadId, string.Join(", ", currentContext.Letters.Select(x => x.Char))));
            });
            return Ok("Done");
        }

        [HttpPost("/pool")]
        public async Task<IActionResult> Pool(char[] letters)
        {
            await Parallel.ForEachAsync(letters, async (letter, cancellationToken) =>
            {
                var threadId = Guid.NewGuid();
                IDbContextPoolable poolable = null;
                while (poolable == null)
                {
                    poolable = _dbContextPool.Rent();
                }

                if (poolable is AppDbContext currentContext)
                {
                    await currentContext.AddAsync(new Letter
                    {
                        Char = letter
                    }, cancellationToken);

                    await currentContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(string.Format("[Thread {0}]: Current elements -> {1}", threadId, string.Join(", ", currentContext.Letters.Select(x => x.Char))));
                }

                await _dbContextPool.ReturnAsync(poolable);
            });
            return Ok("Done");
        }
    }
}
