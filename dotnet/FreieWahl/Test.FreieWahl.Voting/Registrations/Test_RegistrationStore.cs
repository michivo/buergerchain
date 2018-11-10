using System;
using System.Threading.Tasks;
using FreieWahl.Voting.Registrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.FreieWahl.Voting.Registrations
{
    /// <summary>
    /// Summary description for Test_RegistrationStore
    /// </summary>
    [TestClass]
    public class Test_RegistrationStore
    {

        private static readonly string ProjectId = "freiewahl-data";
        private RegistrationStore _votingStore;

        [TestInitialize]
        public void Init()
        {
            _votingStore = new RegistrationStore(ProjectId, RegistrationStore.TestNamespace);
        }

        [TestMethod]
        public async Task AddAndGet()
        {
            var registration = new OpenRegistration
            {
                VoterName = "Michael Faschinger",
                VoterIdentity = "schwurbelschwarbel",
                VotingId = "4294967297123456789",
                RegistrationTime = DateTime.UtcNow
            };

            await _votingStore.AddOpenRegistration(registration);

            Assert.IsFalse(string.IsNullOrEmpty(registration.Id));

            var readRegistration = await _votingStore.GetOpenRegistration(registration.Id);

            Assert.AreEqual(registration.VotingId, readRegistration.VotingId);
            Assert.AreEqual(registration.VoterName, readRegistration.VoterName);
            Assert.AreEqual(registration.VoterIdentity, readRegistration.VoterIdentity);
            Assert.AreEqual(registration.Id, readRegistration.Id);
            Assert.AreEqual(registration.RegistrationTime.Year, readRegistration.RegistrationTime.Year);
            Assert.AreEqual(registration.RegistrationTime.DayOfYear, readRegistration.RegistrationTime.DayOfYear);
            Assert.AreEqual(registration.RegistrationTime.Hour, readRegistration.RegistrationTime.Hour);
            Assert.AreEqual(registration.RegistrationTime.Minute, readRegistration.RegistrationTime.Minute);
            Assert.AreEqual(registration.RegistrationTime.Second, readRegistration.RegistrationTime.Second);
        }
    }
}
