using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ApplicationLayer.Features.Companies.DTOs;
using MediatR;
using System.Collections.Generic;

namespace ApplicationLayer.Features.Companies.Queries.GetMyCompanies;

public record GetMyCompaniesQuery() : IRequest<List<CompanyDto>>;

