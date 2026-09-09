using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OkVolleyVibes.Tests.Authentication;

namespace OkVolleyVibes.Tests.Account;

public sealed class ProfilePhotoTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private static readonly byte[] TinyJpeg =
        [.. new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 }, .. Encoding.ASCII.GetBytes(new string('x', 512))];

    private static MultipartFormDataContent File(byte[] bytes, string contentType, string name = "photo.jpg")
    {
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { content, "file", name } };
    }

    [Fact]
    public async Task Uploading_a_jpeg_stores_it_and_it_can_be_read_back()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("photo.ok@example.com");
        HttpClient client = factory.CreateClient(user.AccessToken);

        HttpResponseMessage upload = await client.PostAsync("/api/account/photo", File(TinyJpeg, "image/jpeg"));
        upload.StatusCode.Should().Be(HttpStatusCode.NoContent);

        JsonElement profile = await client.GetFromJsonAsync<JsonElement>("/api/account/profile");
        profile.GetProperty("hasPhoto").GetBoolean().Should().BeTrue();

        HttpResponseMessage fetched = await client.GetAsync($"/api/account/photo/{user.UserId}");
        fetched.StatusCode.Should().Be(HttpStatusCode.OK);
        fetched.Content.Headers.ContentType!.MediaType.Should().Be("image/jpeg");
        (await fetched.Content.ReadAsByteArrayAsync()).Should().Equal(TinyJpeg);
    }

    [Fact]
    public async Task A_file_that_is_not_an_image_is_rejected()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("photo.bogus@example.com");

        HttpResponseMessage upload = await factory.CreateClient(user.AccessToken)
            .PostAsync("/api/account/photo", File(Encoding.ASCII.GetBytes("hello, not an image"), "image/png"));

        upload.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        JsonElement body = await upload.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errorCode").GetString().Should().Be("validation.failed");
    }

    [Fact]
    public async Task An_oversized_image_is_rejected()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("photo.big@example.com");
        byte[] big = [.. new byte[] { 0xFF, 0xD8, 0xFF }, .. new byte[600 * 1024]];

        HttpResponseMessage upload = await factory.CreateClient(user.AccessToken)
            .PostAsync("/api/account/photo", File(big, "image/jpeg"));

        upload.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Fetching_a_photo_that_was_never_set_is_404()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("photo.none@example.com");

        HttpResponseMessage response = await factory.CreateClient(user.AccessToken)
            .GetAsync($"/api/account/photo/{user.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
