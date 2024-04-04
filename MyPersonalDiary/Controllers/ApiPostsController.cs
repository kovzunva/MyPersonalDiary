using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MyPersonalDiary.Models;
using MyPersonalDiary.Interfaces;

namespace MyPersonalDiary.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class ApiPostsController : Controller
    {
        private readonly IPostsService _postsService;
        private readonly IAccountService _accountService;

        public ApiPostsController(IPostsService postsService, IAccountService accountService)
        {
            _postsService = postsService;
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts([FromHeader(Name = "api_key")] string api_key)
        {
            User currentUser = _accountService.GetUserByApiKey(api_key);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var posts = await _postsService.GetPostsAsync(currentUser);
            return posts;   
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost([FromHeader(Name = "api_key")] string api_key, int id)
        {
            User currentUser = _accountService.GetUserByApiKey(api_key);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var (post, errorMessage) = await _postsService.GetPostAsync(currentUser, id);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return BadRequest(errorMessage);
            }

            return post;
        }

        [HttpPost]
        public async Task<ActionResult<Post>> CreatePost([FromHeader(Name = "api_key")] string api_key, [FromBody] Post post, IFormFile? ImagePath)
        {
            User currentUser = _accountService.GetUserByApiKey(api_key);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            List<string> errors = _postsService.ValidatePost(post);
            if (errors.Count > 0)
            {
                return BadRequest(errors);
            }

            await _postsService.CreatePostAsync(currentUser, post, ImagePath);

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditPost([FromHeader(Name = "api_key")] string api_key, int id, [FromBody] Post post, IFormFile? ImagePath)
        {
            User currentUser = _accountService.GetUserByApiKey(api_key);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var(existingPost, errorMessage) = await _postsService.GetPostAsync(currentUser, id);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return BadRequest(errorMessage);
            }

            List<string> errors = _postsService.ValidatePost(post);
            if (errors.Count > 0)
            {
                return BadRequest(errors);
            }

            await _postsService.EditPostAsync(existingPost, post, ImagePath);
            return Content("Пост успішно відредагованр");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost([FromHeader(Name = "api_key")] string api_key, int id)
        {
            User currentUser = _accountService.GetUserByApiKey(api_key);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var (post, errorMessage) = await _postsService.GetPostAsync(currentUser, id);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return BadRequest(errorMessage);
            }

            await _postsService.DeletePostAsync(post);

            return Content("Пост успішно видалено");
        }
    }
}
