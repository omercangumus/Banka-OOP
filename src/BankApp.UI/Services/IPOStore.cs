using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BankApp.UI.Services
{
    public class IPORequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Symbol { get; set; }
        public string Company { get; set; }
        public int Lot { get; set; }
        public double Price { get; set; }
        public double Total { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    
    public static class IPOStore
    {
        private static readonly string _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NovaBank", "ipo");
        
        private static readonly string _requestsPath = Path.Combine(_basePath, "ipo_requests.json");
        
        static IPOStore()
        {
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }
        
        public static List<IPORequest> LoadRequests()
        {
            try
            {
                if (!File.Exists(_requestsPath))
                    return new List<IPORequest>();
                
                var json = File.ReadAllText(_requestsPath);
                return JsonSerializer.Deserialize<List<IPORequest>>(json) ?? new List<IPORequest>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IPO Store Load Error: {ex.Message}");
                
                // Backup corrupted file
                if (File.Exists(_requestsPath))
                {
                    try
                    {
                        File.Copy(_requestsPath, _requestsPath + ".bak", true);
                    }
                    catch { }
                }
                
                return new List<IPORequest>();
            }
        }
        
        public static bool SaveRequest(IPORequest request)
        {
            try
            {
                var requests = LoadRequests();
                requests.Add(request);
                
                return AtomicWrite(requests);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IPO Store Save Error: {ex.Message}");
                return false;
            }
        }
        
        public static bool UpdateRequestStatus(string id, string status)
        {
            try
            {
                var requests = LoadRequests();
                var request = requests.FirstOrDefault(r => r.Id == id);
                if (request == null) return false;
                
                request.Status = status;
                
                return AtomicWrite(requests);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IPO Store Update Error: {ex.Message}");
                return false;
            }
        }
        
        private static bool AtomicWrite(List<IPORequest> requests)
        {
            var tempPath = _requestsPath + ".tmp";
            try
            {
                var json = JsonSerializer.Serialize(requests, new JsonSerializerOptions { WriteIndented = true });
                
                // Write to temp file first
                File.WriteAllText(tempPath, json);
                
                // Replace original file atomically
                if (File.Exists(_requestsPath))
                    File.Delete(_requestsPath);
                File.Move(tempPath, _requestsPath);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IPO Store Atomic Write Error: {ex.Message}");
                
                // Clean up temp file if exists
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
                
                return false;
            }
        }
        
        public static List<IPORequest> GetPendingRequests()
        {
            return LoadRequests().Where(r => r.Status == "Pending").ToList();
        }
    }
}
