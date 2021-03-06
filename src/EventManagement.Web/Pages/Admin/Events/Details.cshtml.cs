using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using losol.EventManagement.Domain;
using losol.EventManagement.Infrastructure;
using static losol.EventManagement.Domain.Registration;
using static losol.EventManagement.Domain.Order;

namespace losol.EventManagement.Pages.Admin.Events
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public EventInfo EventInfo { get; set; }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EventInfo = await _context.EventInfos
                .Include(e => e.Products)
                    .ThenInclude(p => p.ProductVariants)
                .Include(e => e.Registrations)
                .SingleOrDefaultAsync(m => m.EventInfoId == id);

            if (EventInfo == null)
            {
                return NotFound();
            }

            return Page();
        }


        public async Task<JsonResult> OnGetParticipants(int? id)
        {
            if (id == null)
            {
               return new JsonResult("No event id submitted.");
            }

            var registrations = await _context.Registrations
                .Where(
                    r => r.EventInfoId == id &&
                    r.Status != RegistrationStatus.Cancelled &&
                    r.Type == RegistrationType.Participant)
                .Include(r => r.Orders)
                    .ThenInclude(o => o.OrderLines)
                        .ThenInclude(ol => ol.Product)
                .Include(r => r.Orders)
                    .ThenInclude(o => o.OrderLines)
                        .ThenInclude(ol => ol.ProductVariant)
                .Include(r => r.User)
                .ToListAsync();
            var vms = registrations.Select(x => new RegistrationsVm
            {
                RegistrationId = x.RegistrationId,
                Name = x.User.Name,
                Email = x.User.Email,
                Phone = x.User.PhoneNumber,
                JobTitle = x.ParticipantJobTitle,
                Notes = x.Notes,
                Employer = x.ParticipantEmployer,
                City = x.ParticipantCity,
                Products = x.Products.Select(dto => ValueTuple.Create(
                    new RegistrationsProductVm(dto.Product),
                    RegistrationsVariantVm.Create(dto.Variant),
                    dto.Quantity)).ToList(),
                HasCertificate = x.HasCertificate,
                CertificateId = x.CertificateId,
                Status = x.Status.ToString(),
                Type = x.Type.ToString()
            });

            if (vms.Any()) {
                return new JsonResult(vms);
            }
            else {
                return new JsonResult("none");
            }
        }

        public async Task<JsonResult> OnGetOtherAttendees(int? id)
        {
            if (id == null)
            {
               return new JsonResult("No event id submitted.");
            }

            var registrations = await _context.Registrations
                .Where(
                    r => r.EventInfoId == id &&
                    r.Status != RegistrationStatus.Cancelled &&
                    r.Type != RegistrationType.Participant)
                .Include(r => r.Orders)
                    .ThenInclude(o => o.OrderLines)
                        .ThenInclude(ol => ol.Product)
                .Include(r => r.Orders)
                    .ThenInclude(o => o.OrderLines)
                        .ThenInclude(ol => ol.ProductVariant)
                .Include(r => r.User)
                .ToListAsync();
            var vms = registrations.Select(x => new RegistrationsVm
            {
                RegistrationId = x.RegistrationId,
                Name = x.User.Name,
                Email = x.User.Email,
                Phone = x.User.PhoneNumber,
                JobTitle = x.ParticipantJobTitle,
                Employer = x.ParticipantEmployer,
                City = x.ParticipantCity,
                Notes = x.Notes,
                Products = x.Products.Select(dto => ValueTuple.Create(
                    new RegistrationsProductVm(dto.Product),
                    RegistrationsVariantVm.Create(dto.Variant),
                    dto.Quantity)).ToList(),
                HasCertificate = x.HasCertificate,
                CertificateId = x.CertificateId,
                Status = x.Status.ToString(),
                Type = x.Type.ToString()
            });

            if (vms.Any()) {
                return new JsonResult(vms);
            }
            else {
                return new JsonResult("none");
            }
        }

        public async Task<JsonResult> OnGetCancelled(int? id)
        {
            if (id == null)
            {
               return new JsonResult("No event id submitted.");
            }

            var registrations = await _context.Registrations
                .Where(
                    r => r.EventInfoId == id &&
                    r.Status == RegistrationStatus.Cancelled)
                .Select ( x=> new RegistrationsVm{
                    RegistrationId = x.RegistrationId,
                    Name = x.User.Name,
                    Email = x.User.Email,
                    Phone = x.User.PhoneNumber,
                    Notes = x.Notes,
                    JobTitle = x.ParticipantJobTitle,
                    Employer = x.ParticipantEmployer,
                    City = x.ParticipantCity,
                    HasCertificate = x.HasCertificate,
                    CertificateId = x.CertificateId,
                    Status = x.Status.ToString(),
                    Type = x.Type.ToString()
                    })
                .ToListAsync();

            if (registrations.Any()) {
                return new JsonResult(registrations);
            }
            else {
                return new JsonResult("none");
            }
        }

        internal class RegistrationsVm
        {
            public int RegistrationId {get;set;}
            public string Name { set;get;}
            public string Email { set;get;}
            public string Phone { set;get;}
            public string Employer {get;set;}
            public string Notes {get;set;}
            public string JobTitle {get;set;}
            public string City {get;set;}
            public bool HasCertificate { get; set; }
            public int? CertificateId {get; set; }
            public string Status {get;set;}
            public string Type {get;set;}
            // public List<(Product, ProductVariant, int)> Products { get; set; }
            public List<(RegistrationsProductVm, RegistrationsVariantVm, int)> Products { get; set; }
        }

        internal class RegistrationsProductVm
        {
            public int ProductId { get; set; }
            public string Name { get; set; }

            public RegistrationsProductVm(Product product)
            {
                this.ProductId = product.ProductId;
                this.Name = product.Name;
            }
        }

        internal class RegistrationsVariantVm
        {
            public int ProductVariantId { get; set; }
            public string Name { get; set; }

            public RegistrationsVariantVm(ProductVariant variant)
            {
                this.ProductVariantId = variant.ProductVariantId;
                this.Name = variant.Name;
            }
            public static RegistrationsVariantVm Create(ProductVariant variant) =>
                variant == null ? null : new RegistrationsVariantVm(variant);
        }

    }
}
