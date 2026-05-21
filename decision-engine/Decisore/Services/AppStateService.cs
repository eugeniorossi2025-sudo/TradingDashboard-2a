using System;
using System.Collections.Generic;
using Decisore.Repository;

namespace Decisore.Services
{
    public class AppStateService
    {
        private DateTime? _decideStartUtc;
        private Dictionary<string, string> _configurations;

        private readonly IServiceScopeFactory _scopeFactory;

        public AppStateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            LoadConfigurations();
        }

        /* ---------------- CONFIGURAZIONI ---------------- */

        public void LoadConfigurations()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var _db = scope.ServiceProvider
                    .GetRequiredService<DatabaseRepository>();
                
                _configurations = _db.GetConfigurations();
            }
            catch
            {
                _configurations = new Dictionary<string, string>();
            }
        }

        public Dictionary<string, string> Configurations => _configurations;

        /* ---------------- ELAPSED SERVER ---------------- */

        public void ResetElapsed()
        {
            _decideStartUtc = null;
        }

        public double GetElapsedMinutes()
        {
            if (_decideStartUtc == null)
            {
                _decideStartUtc = DateTime.UtcNow;
                return 0;
            }

            return (DateTime.UtcNow - _decideStartUtc.Value).TotalMinutes;
        }
    }
}