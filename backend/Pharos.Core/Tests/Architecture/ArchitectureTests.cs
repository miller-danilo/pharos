using NetArchTest.Rules;
using Xunit;

namespace Pharos.Core.Tests.Architecture
{
    public class ArchitectureTests
    {
        [Fact]
        public void CoreLayer_ShouldNot_DependOnPresentationLayer()
        {
            var result = Types.InAssembly(typeof(Pharos.Core.Services.FirestoreService).Assembly)
                .That().ResideInNamespace("Pharos.Core")
                .ShouldNot()
                .HaveDependencyOn("Pharos.Api")
                .GetResult();

            Assert.True(result.IsSuccessful, "SRP Violation: Pharos.Core cannot reference Pharos.Api presentation elements.");
        }

        [Fact]
        public void Controllers_ShouldNot_DependOnConcreteServices()
        {
            var result = Types.InAssembly(typeof(Pharos.Api.Controllers.ProposalController).Assembly)
                .That().ResideInNamespace("Pharos.Api.Controllers")
                .ShouldNot()
                .HaveDependencyOn("Pharos.Core.Services.GeminiService")
                .GetResult();

            var result2 = Types.InAssembly(typeof(Pharos.Api.Controllers.ProposalController).Assembly)
                .That().ResideInNamespace("Pharos.Api.Controllers")
                .ShouldNot()
                .HaveDependencyOn("Pharos.Core.Services.FirestoreService")
                .GetResult();

            Assert.True(result.IsSuccessful, "DIP Violation: Controllers must depend on interface abstractions, not concrete GeminiService.");
            Assert.True(result2.IsSuccessful, "DIP Violation: Controllers must depend on interface abstractions, not concrete FirestoreService.");
        }
    }
}
