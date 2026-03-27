using ExaminationSystem.API.Middlewares;
using ExaminationSystem.Application.Common;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Infrastructure.Configs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;

namespace ExaminationSystem.UnitTests.Services;

public class TenantResolutionMiddlewareTests
{
    private readonly Mock<ITenantDomainResolver> _resolverMock;
    private readonly Mock<ITenantAccessor> _accessorMock;
    private readonly Mock<ILogger<TenantResolutionMiddleware>> _loggerMock;

    public TenantResolutionMiddlewareTests()
    {
        _resolverMock = new Mock<ITenantDomainResolver>();
        _accessorMock = new Mock<ITenantAccessor>();
        _loggerMock = new Mock<ILogger<TenantResolutionMiddleware>>();
    }

    #region Happy Path Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task InvokeAsync_KnownDomain_SetsTenantAndCallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var context = CreateHttpContext("tenant1.example.com", "/api/exams");
        _resolverMock.Setup(x => x.ResolveTenantIdByDomainAsync("tenant1.example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        await middleware.InvokeAsync(context, _resolverMock.Object, _accessorMock.Object);

        // Assert
        nextCalled.Should().BeTrue();
        _accessorMock.Verify(x => x.SetTenantId(5), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task InvokeAsync_ExcludedPath_SkipsTenantResolution()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateHttpContext("unknown.com", "/health");

        // Act
        await middleware.InvokeAsync(context, _resolverMock.Object, _accessorMock.Object);

        // Assert
        nextCalled.Should().BeTrue();
        _resolverMock.Verify(x => x.ResolveTenantIdByDomainAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _accessorMock.Verify(x => x.SetTenantId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task InvokeAsync_HangfirePath_SkipsTenantResolution()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateHttpContext("unknown.com", "/hangfire/dashboard");

        // Act
        await middleware.InvokeAsync(context, _resolverMock.Object, _accessorMock.Object);

        // Assert
        nextCalled.Should().BeTrue();
        _resolverMock.Verify(x => x.ResolveTenantIdByDomainAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Unknown Domain Tests

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task InvokeAsync_UnknownDomain_RejectRequest_Returns400()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            unknownAction: UnknownDomainAction.RejectRequest);

        var context = CreateHttpContext("unknown.com", "/api/exams");
        _resolverMock.Setup(x => x.ResolveTenantIdByDomainAsync("unknown.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        // Act
        await middleware.InvokeAsync(context, _resolverMock.Object, _accessorMock.Object);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(400);
        _accessorMock.Verify(x => x.SetTenantId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task InvokeAsync_UnknownDomain_UseDefaultTenant_SetsFallbackTenant()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            unknownAction: UnknownDomainAction.UseDefaultTenant,
            defaultTenantId: 99);

        var context = CreateHttpContext("unknown.com", "/api/exams");
        _resolverMock.Setup(x => x.ResolveTenantIdByDomainAsync("unknown.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        // Act
        await middleware.InvokeAsync(context, _resolverMock.Object, _accessorMock.Object);

        // Assert
        nextCalled.Should().BeTrue();
        _accessorMock.Verify(x => x.SetTenantId(99), Times.Once);
    }

    #endregion

    #region Helpers

    private TenantResolutionMiddleware CreateMiddleware(
        RequestDelegate? next = null,
        UnknownDomainAction unknownAction = UnknownDomainAction.RejectRequest,
        int defaultTenantId = 1)
    {
        next ??= _ => Task.CompletedTask;
        var config = new TenancyConfig
        {
            UnknownDomainAction = unknownAction,
            DefaultTenantId = defaultTenantId,
            ExcludedPaths = new[] { "/health", "/hangfire" }
        };

        return new TenantResolutionMiddleware(
            next,
            Options.Create(config),
            _loggerMock.Object);
    }

    private static DefaultHttpContext CreateHttpContext(string host, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(host);
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    #endregion
}
