using AttributeRouting.Web.Mvc;
using AccountManager.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using System.Web;

namespace AccountManager.Controllers
{
    public class AccountController : ApiController
    {

        private static AccountContext _db = new AccountContext();
        private static AccountModelValidation acct_model = new AccountModelValidation();
        private static CharacterModelValidation char_model = new CharacterModelValidation();

        private string FirstToUpper(string s)
        {
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private async Task<WarcraftAcct> GetAccountID(string acct_name)
        {
            if (acct_model.AccountEntryIsValid(acct_name))
            {
                try
                {
                    List<WarcraftAcct> account_entry = await (from q in _db.WarcraftAccts
                                                              where q.account_name.ToLower() == acct_name.ToLower()
                                                              select q).ToListAsync();
                    return account_entry.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;
        }

        private async Task<List<Character>> GetActiveCharacterList(int account_id)
        {
            List<Character> char_list = await (from q in _db.Characters
                                               where q.account_id == account_id &&
                                              q.active == true
                                               select q).ToListAsync();
            return char_list;
        }
        private async Task<List<Character>> GetAllCharacterList(int account_id)
        {
            List<Character> char_list = await (from q in _db.Characters
                                               where q.account_id == account_id
                                               orderby q.active descending,
                                               q.character_id descending
                                               select q).ToListAsync();
            return char_list;
        }

        //returns a list of all accounts 
        //deleted accounts are marked active = false in the return body
        [GET("Accounts")]
        public async Task<object> Get()
        {
            //due to the future potential size of some of these queries scalability
            //is a definite concern and this would need redesigned as such 
            List<WarcraftAcct> account_list = await _db.WarcraftAccts.ToListAsync();
            account_list.ForEach(a => a.link = buildURL(a.account_name));
            return new { accounts = account_list };
        }

        //check for needing to add the trailing forward slash of absolutepath
        private string buildURL(string s)
        {
            string insert = Request.RequestUri.Scheme + "://"
                               + Request.RequestUri.Authority
                               + Request.RequestUri.AbsolutePath;
            if (insert.EndsWith("/"))
            {
                return insert + s;
            }
            return insert + "/" + s;
        }

        [GET("Accounts/{account_name}/characters")]
        public async Task<object> GetCharacters(string account_name)
        {
            WarcraftAcct account_entry = await GetAccountID(account_name);
            //does not return deleted accounts characters
            if (account_entry != null && account_entry.active == true)
            {
                return new
                {
                    account_id = account_entry.account_id,
                    characters = await GetActiveCharacterList(account_entry.account_id)
                };
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Account Error: Does Not Exist/Inactive");
        }

        [POST("Accounts")]
        public async Task<object> CreateAccount(HttpRequestMessage requestBody)
        {
            try
            {
                string body = requestBody.Content.ReadAsStringAsync().Result;
                NewAccount deserial_account = null;

                //build a new entry for account table
                WarcraftAcct new_entry = new WarcraftAcct();
                deserial_account = JsonConvert.DeserializeObject<NewAccount>(body);

                //try to map JSON to account obj will throw on invalid
                //inputs or format
                if (body.Contains(",") || deserial_account == null ||
                    !acct_model.AccountEntryIsValid(deserial_account.name))
                {
                    throw new InvalidOperationException();
                }
                new_entry.account_name = FirstToUpper(deserial_account.name);
                new_entry.active = true;

                List<int> account_match = await (from q in _db.WarcraftAccts
                                                 where q.account_name == new_entry.account_name
                                                 select q.account_id).ToListAsync();
                //check for name collisions

                if (account_match.FirstOrDefault() == 0)
                {
                    _db.WarcraftAccts.Add(new_entry);
                    await _db.SaveChangesAsync();
                    return new { account_id = new_entry.account_id };
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request Error: Account or Request Body Invalid");
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Account Error: Account Name Already Exists");
        }

        [POST("Accounts/{account_name}/characters")]
        public async Task<object> CreateCharacter(string account_name, HttpRequestMessage body)
        {

            try
            {
                //try to map JSON to character obj will throw on invalid
                //inputs or format
                Character char_entry = null;
                char_entry = JsonConvert.DeserializeObject<Character>(body.Content.ReadAsStringAsync().Result);
                if (char_entry == null || !char_model.CharEntryIsValid(char_entry))
                {
                    throw new InvalidOperationException();
                }

                //make input easier on the user
                char_entry.name = FirstToUpper(char_entry.name.ToLower());

                //only allow character creation if account has not been deleted
                //and account exists
                WarcraftAcct account_entry = await GetAccountID(account_name);
                if (account_entry == null || account_entry.active == false)
                {
                    throw new InvalidOperationException();
                }
                //continue building new character entry
                char_entry.account_id = account_entry.account_id;
                char_entry.active = true;
                char_model.UpdateCasing(char_entry);
                //does the character meet faction/class/race/level requirements
                List<bool> cs = await (from q in _db.CharSelections
                                       where q.faction == char_entry.faction &&
                                             q.race == char_entry.race &&
                                             q.@class == char_entry.@class &&
                                             q.startlvl <= char_entry.level &&
                                             q.maxlvl >= char_entry.level
                                       select q.enabled).ToListAsync();

                if (cs.FirstOrDefault())
                {
                    List<Character> active_charList = await GetActiveCharacterList(char_entry.account_id);
                    if (active_charList != null)
                    {
                        foreach (Character c in active_charList)
                        {
                            if (c.faction != char_entry.faction || c.name == char_entry.name)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Character Conflict: Name/Faction");
                            }
                        }
                    }
                    _db.Characters.Add(char_entry);
                    await _db.SaveChangesAsync();
                    return new
                    {
                        character_id = char_entry.character_id
                    };
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request Error: Account or Request Body Invalid");
            }

            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Character Conflict: Invalid Character Option");
        }



        [DELETE("Accounts/{account_name}")]
        public async Task<object> Delete(string account_name)
        {
            WarcraftAcct account_entry = await GetAccountID(account_name);
            if (account_entry != null)
            {
                //if account exists delete
                //if account exists and deleted then undelete
                account_entry.active = !account_entry.active;
                _db.Entry(account_entry).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return new { account = FirstToUpper(account_name), active = account_entry.active };
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request Error: Account Invalid");
        }

        [DELETE("Accounts/{account_name}/characters/{character_name}")]
        public async Task<object> DeleteCharacter(string account_name, string character_name)
        {
            WarcraftAcct account_entry = await GetAccountID(account_name);
            //only allow character manipulation if account is not deleted
            if (account_entry != null && account_entry.active == true)
            {
                List<Character> account_CharList = await GetAllCharacterList(account_entry.account_id);

                //go through all the past characters to check for 
                //the most recent character or most recent deleted character
                //matching the given name
                string faction = "";
                if (account_CharList != null)
                {
                    if (account_CharList[0].active == true)
                    {
                        faction = account_CharList[0].faction;
                    }
                    foreach (Character c in account_CharList)
                    {
                        if (c.name == FirstToUpper(character_name.ToLower())
                            && (faction.Length == 0 || faction == c.faction))
                        {
                            //if char exists delete
                            //if char exists and deleted then undelete

                            c.active = !c.active;
                            _db.Entry(c).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            return new
                            {
                                account = FirstToUpper(account_entry.account_name),
                                character = FirstToUpper(character_name.ToLower()),
                                active = c.active
                            };
                        }
                    }
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request Error: Account or Character Invalid");
        }


    }

}