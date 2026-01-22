using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using ApplicationLayer.Features.Companies.DTOs;

namespace ApplicationLayer.Features.Companies.Queries.GetCompanyById;

public record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto?>;
