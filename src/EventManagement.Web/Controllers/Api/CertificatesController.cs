using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using F = System.IO.File;

using System.IO;
using System.Text;
using losol.EventManagement.Domain;
using losol.EventManagement.Services;
using losol.EventManagement.Services.Messaging;
using losol.EventManagement.ViewModels;
using losol.EventManagement.Web.Services;
using losol.EventManagement.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace losol.EventManagement.Web.Controllers.Api {

    [Authorize (Policy = "AdministratorRole")]
    [Route ("api/certificates")]
    public class CertificatesController : Controller {

        private readonly IRegistrationService _registrationService;
        private readonly ICertificatesService _certificatesService;

        public CertificatesController (IRegistrationService registrationService, ICertificatesService certificatesService) {
            _registrationService = registrationService;
            _certificatesService = certificatesService;
        }

        [HttpPost ("event/{eventId}/email")]
        public async Task<IActionResult> GenerateCertificatesAndSendEmails ([FromRoute] int eventId, [FromServices] CertificatePdfRenderer writer, [FromServices] StandardEmailSender emailSender) {
            var certificates = await _certificatesService.CreateCertificatesForEvent(eventId);

            foreach( var certificate in certificates ) {
                var result = await writer.RenderAsync( CertificateVM.From ( certificate ) );
                var memoryStream = new MemoryStream ();
                await result.CopyToAsync (memoryStream);
                await emailSender.SendStandardEmailAsync (new EmailMessage {
                    Email = certificate.RecipientEmail,
                        Subject = $"Kursbevis for {certificate.Title}",
                        Message = "Her er kursbeviset! Gratulere!",
                        Attachment = new Attachment { Filename = "kursbevis.pdf", Bytes = memoryStream.ToArray () }
                });
            }
            return Ok ();
        }

        [HttpPost ("registration/{regId}/email")]
        public async Task<IActionResult> EmailCertificate ([FromRoute] int regId, [FromServices] CertificatePdfRenderer writer, [FromServices] StandardEmailSender emailSender) {
            var c = await _certificatesService.GetForRegistrationAsync (regId);
            var result = await writer.RenderAsync (CertificateVM.From (c));
            var memoryStream = new MemoryStream ();
            await result.CopyToAsync (memoryStream);
            var emailMessage = new EmailMessage {
                Email = c.RecipientEmail,
                Subject = $"Kursbevis for {c.Title}",
                Message = "Her er kursbeviset! Gratulere!",
                Attachment = new Attachment {
                Filename = "kursbevis.pdf",
                Bytes = memoryStream.ToArray ()
                }
            };
            await emailSender.SendStandardEmailAsync (emailMessage);
            return Ok ();
        }
    }
}