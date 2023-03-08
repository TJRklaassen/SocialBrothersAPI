using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SocialBrothersApi.Models;

namespace SocialBrothersApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly AddressContext _context;

        public AddressController(AddressContext context)
        {
            _context = context;
        }

        // GET: api/address
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
        {
            if (_context.Addresses == null)
            {
                return NotFound();
            }
            return await _context.Addresses.ToListAsync();
        }

        // GET: api/address/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddress(long id)
        {
            if (_context.Addresses == null)
            {
                return NotFound();
            }
            var address = await _context.Addresses.FindAsync(id);

            if (address == null)
            {
                return NotFound();
            }

            return address;
        }

        // GET: api/address/filter/dreef
        [HttpGet("filter/{value}")]
        public async Task<ActionResult<IEnumerable<Address>>> GetFilteredAddresses(string value)
        {
            if (_context.Addresses == null)
            {
                return NotFound();
            }

            var addresses = await _context.Addresses.ToListAsync();
            var filteredAddresses = addresses.Where(a => AddressHasValue(a, value));

            if (!filteredAddresses.Any())
            {
                return NotFound();
            }

            return Ok(filteredAddresses);
        }

        // GET: api/address/ascending/street
        [HttpGet("sort/{direction}/{field}")]
        public async Task<ActionResult<IEnumerable<Address>>> GetSortedAddresses(string direction, string field)
        {
            if (_context.Addresses == null)
            {
                return NotFound();
            }
            
            // Check if the specified field is a valid Address field
            PropertyInfo property = typeof(Address).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                return BadRequest("Invalid field specified");
            }

            var addresses = await _context.Addresses.ToListAsync();

            // Sort the addresses based on the specified order direction and field
            if (direction.ToLower().Equals("ascending"))
            {
                addresses = addresses.OrderBy(a => property.GetValue(a, null)).ToList();
            }
            else if (direction.ToLower().Equals("descending"))
            {
                addresses = addresses.OrderByDescending(a => property.GetValue(a, null)).ToList();
            }
            else
            {
                return BadRequest("Invalid order direction specified");
            }

            return Ok(addresses);
        }

        // GET: api/address/distance/1/3
        [HttpGet("distance/{id1}/{id2}")]
        public async Task<ActionResult<double>> GetAddressDistance(long id1, long id2)
        {
            if (_context.Addresses == null)
            {
                return NotFound();
            }

            var address1 = await _context.Addresses.FindAsync(id1);
            var address2 = await _context.Addresses.FindAsync(id2);

            if (address1 == null || address2 == null)
            {
                return NotFound();
            }

            var (lat1, lon1) = await FindCoordinates(address1);
            var (lat2, lon2) = await FindCoordinates(address2);
            
            var distance = CalculateDistance(lat1, lon2, lat2, lon2);

            return distance;
        }

        // PUT: api/address/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAddress(long id, Address address)
        {
            if (id != address.Id)
            {
                return BadRequest();
            }

            // This validation has to happen manually, since int Number defaults to 0 if not specified
            if (address.Number <= 0)
            {
                ModelState.AddModelError("Number", "Number is required and must be greater than 0.");
                return BadRequest(ModelState);
            }

            _context.Entry(address).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AddressExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/address
        [HttpPost]
        public async Task<ActionResult<Address>> PostAddress(Address address)
        {
            if (_context.Addresses == null)
            {
                return Problem("Entity set 'AddressContext.Addresses'  is null.");
            }

            // This validation has to happen manually, since int Number defaults to 0 if not specified
            if (address.Number <= 0)
            {
                ModelState.AddModelError("Number", "Number is required and must be greater than 0.");
                return BadRequest(ModelState);
            }

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAddress", new { id = address.Id }, address);
        }

        // DELETE: api/address/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(long id)
        {
            if (_context.Addresses == null)
            {
                return NotFound();
            }
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AddressExists(long id)
        {
            return (_context.Addresses?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // Checks if any field inside the given address contains the searchValue
        private bool AddressHasValue(Address address, string searchValue)
        {
            var type = address.GetType();
            foreach (var field in type.GetProperties())
            {
                var value = field.GetValue(address)?.ToString().ToLower();
                if (value != null && value.Contains(searchValue.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        // Connects to OpenStreetMap's geolocation API to find the coordinates of specified address
        private async Task<(double lat, double lon)> FindCoordinates(Address address)
        {
            string addressString = $"{address.Street} {address.Number}, {address.ZipCode} {address.City}";
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Social Brothers API");
            var response = await httpClient.GetAsync($"https://nominatim.openstreetmap.org/search?q={addressString}&format=json");
            var content = await response.Content.ReadAsStringAsync();
            var result = JArray.Parse(content).FirstOrDefault() as JObject;

            if (result != null)
            {
                var lat = double.Parse(result["lat"].ToString(), CultureInfo.InvariantCulture);
                var lon = double.Parse(result["lon"].ToString(), CultureInfo.InvariantCulture);

                return (lat, lon);
            }
            else
            {
                throw new Exception($"Failed to geocode address: {addressString}");
            }
        }

        // Calculate the difference in km between coordinates
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = Deg2Rad(lat2 - lat1);
            double dLon = Deg2Rad(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Deg2Rad(lat1)) * 
                Math.Cos(Deg2Rad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = 6371.0088 * c;
            return distance;
        }

        private double Deg2Rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
