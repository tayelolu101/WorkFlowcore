using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WorkFlowCore.DTOs;
using WorkFlowCore.Models;

namespace WorkFlowCore.Controllers
{
    [RoutePrefix("api/GetLocations")]
    public class ATMMangerController : ApiController
    {
        private ApplicationDbContext _context;
        private ApplicationDbContext _contextLoc;

        LogWriter logwriter = new LogWriter();
        public ATMMangerController()
        {
            _context = new ApplicationDbContext("ATMDbConnectionString");
            _contextLoc = new ApplicationDbContext("LocationsDbConnectionString");
        }

        [Route("")]
        public IHttpActionResult GetATMLocation()
        {
            var rStructure = _context.ATMLocationsGeos.ToList();   
            return Ok(rStructure);
        }

        [Route("ByUniqueID/{id}")]
        public IHttpActionResult GetATMLocationByID(string id)
        {
            var rStructure = _context.ATMLocationsGeos.Where(g => g.unique_id == id).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

        [Route("ByCountry/{country}")]
        public IHttpActionResult GetATMLocationByCountry(string country)
        {
            var rStructure = _context.ATMLocationsGeos.Where(g => g.country == country).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }
           
            return Ok(rStructure);
        }

        [Route("ByCountry/{country}/{state}")]
        public IHttpActionResult GetATMLocationByCountry(string country, string state)
        {
            var rStructure = _context.ATMLocationsGeos.Where(g => g.country == country && g.state == state).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

        [Route("ByCountry/{country}/{state}/{city}")]
        public IHttpActionResult GetATMLocationByCountry(string country, string state, string city)
        {
            var rStructure = _context.ATMLocationsGeos.Where(g => g.country == country && g.state == state).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

        [Route("ByStates")]
        public IHttpActionResult GetStates()
        {
            var rStructure = _contextLoc.state_listing.ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

        [Route("LGAByStateCode/{state_id}")]
        public IHttpActionResult GetLGABySate(string state_id)
        {
            var rStructure = _contextLoc.LGA_Listing.Where(l => l.STATE_CODE.Equals(state_id.ToUpper())).ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

        [Route("LGAs")]
        public IHttpActionResult GetLGAS()
        {
            var rStructure = _contextLoc.LGA_Listing.ToList();

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }


        [Route("Countries")]
        public IHttpActionResult GetCountries()
        {

            logwriter.WriteTolog("Error in Get countries");



            var rStructure = _contextLoc.country.ToList();
            //var rStructure = _contextLoc.Database.SqlQuery<Countries>("select * from COUNTRY_LISTING");

            if (rStructure == null)
            {
                return NotFound();
            }

            return Ok(rStructure);
        }

        //[HttpPost]
        //public IHttpActionResult CreateATMLocation(ATMLocationsGeo newatm)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    _context.ATMLocationsGeos.Add(newatm);

        //    _context.SaveChanges();

        //    return Created(new Uri(Request.RequestUri + "/" + newatm.unique_id) , newatm);
        //}

        [HttpPost]
        [Route("CreateATMLocation")]
        public IHttpActionResult CreateATMLocation(ATMLocationsGeoDto atmLocationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var newLocation = Mapper.Map<ATMLocationsGeoDto, ATMLocationsGeo>(atmLocationDto);

            newLocation.create_date = DateTime.Now;
            var cto = atmLocationDto.unique_id;
            _context.ATMLocationsGeos.Add(newLocation);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

                logwriter.WriteTolog("Error in inner exception: " + ex.InnerException);
                logwriter.WriteTolog("Error in message: " + ex.Message);
            }
            

            atmLocationDto.unique_id = newLocation.unique_id;

            return Created(new Uri(Request.RequestUri + "/" + newLocation.unique_id), atmLocationDto);
        }






    }
}
