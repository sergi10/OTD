using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace OTD.Models
{
    class Config
    {
        #region Attributes
        //private Realm _dblocal;
        private static Config _config = new Config( );
        private string baseURI { get { return "http://192.168.0.5:88/SCB"; } }
        private string methodURI { get { return "/CODE2.PHP"; } }
        #endregion
        // Singleton
        public static Config Instance
        {
            get
            {
                if (_config == null)
                    _config = new Config( );
                return _config;
            }
        }

  //      /// <summary>
		///// Gets the realm config.
		///// </summary>
		///// <returns>The realm config.</returns>
		//public RealmConfiguration get_realm_config( )
  //      {
  //          //return new RealmConfiguration("YMlocal.realm") { SchemaVersion = 11 };
  //          return new RealmConfiguration("otd.realm") { SchemaVersion = 5 };
  //      }

  //      /// <summary>
  //      /// Gets the Realm db local.
  //      /// </summary>
  //      /// <returns>The db local.</returns>
  //      public Realm get_db_local( )
  //      {
  //          if (_dblocal != null)
  //          {
  //              return _dblocal;
  //          }
  //          else
  //          {
  //              _dblocal = Realm.GetInstance(this.get_realm_config( ));
  //              return this._dblocal;
  //          }

  //      }
    }
}