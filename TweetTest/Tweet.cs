using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualBasic;
using CoreTweet;
using System.Windows.Forms;

namespace TweetTest
{
    /// <summary>
    /// 渡された文字列をツイートする検証用クラス
    /// </summary>
    class Tweet
    {
        // トークン保存場所
        private static string FileName = @Application.UserAppDataPath + @"\user";

        public static void TweetTest(string text)
        {
            Tokens tokens;
            string consumerKey = Properties.Settings.Default.ConsumerKey;
            string consumerSecret = Properties.Settings.Default.ConsumerSecret;

            // ユーザデータの存在確認。無かったら認証へ
            if (!System.IO.File.Exists(FileName))
            {
                var session = OAuth.Authorize(consumerKey,consumerSecret);
                string url = session.AuthorizeUri.AbsoluteUri;
                System.Diagnostics.Process.Start(url);

                string pinCode = Interaction.InputBox("PINコードを入力", "", "", -1, -1);
                if(pinCode.Length < 7)
                {
                    return;
                }

                tokens = OAuth.GetTokens(session,pinCode);

                // ユーザデータの保存
                TweetUserSerialize.Serialize(new TweetUser(tokens.UserId,tokens.ScreenName,tokens.AccessToken,tokens.AccessTokenSecret));

            }
            else
            {
                // ユーザデータからトークン作成
                TweetUser user = TweetUserSerialize.Deserialize();
                string accessToken = user.AccessToken1;
                string accessTokenSecret = user.AccessTokenSecret1;
                tokens = Tokens.Create(consumerKey, consumerSecret, accessToken, accessTokenSecret);

                // ScreenNameを動的に取得、設定
                var res = tokens.Account.VerifyCredentials();
                tokens.ScreenName = res.ScreenName;
            }

            var result = MessageBox.Show(
                "ツイートしてもよろしいですか？\r\r" + tokens.ScreenName + ":\r" + text,
                "確認",
                MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                tokens.Statuses.Update(new { status = text });
            }

        }
        
    }

    /// <summary>
    /// ユーザデータ保存用クラス
    /// 要らなくなるかも
    /// </summary>
    [Serializable]
    class TweetUser
    {
        private long UserID;
        private string ScreenName;
        private string AccessToken;      
        private string AccessTokenSecret;

        public long UserID1
        {
            get
            {
                return UserID;
            }

            set
            {
                UserID = value;
            }
        }

        public string ScreenName1
        {
            get
            {
                return ScreenName;
            }

            set
            {
                ScreenName = value;
            }
        }

        public string AccessToken1
        {
            get
            {
                return AccessToken;
            }

            set
            {
                AccessToken = value;
            }
        }

        public string AccessTokenSecret1
        {
            get
            {
                return AccessTokenSecret;
            }

            set
            {
                AccessTokenSecret = value;
            }
        }

        public TweetUser(long UserID,string ScreenName,string AccessToken,string AccessTokenSecret)
        {
            this.UserID = UserID;
            this.ScreenName = ScreenName;
            this.AccessToken = AccessToken;
            this.AccessTokenSecret = AccessTokenSecret;
        }
        

    }

    /// <summary>
    /// ユーザデータのシリアライズ用クラス
    /// 要らなくなるかも
    /// </summary>
    class TweetUserSerialize
    {
        private static string FileName = Application.UserAppDataPath + @"\user";
        public static void Serialize(TweetUser user)
        {
            

            BinaryFormatter bf = new BinaryFormatter();

            System.IO.FileStream fs = new System.IO.FileStream(FileName, System.IO.FileMode.Create);

            bf.Serialize(fs,user);
            fs.Close();
        }

        public static TweetUser Deserialize()
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream fs = new System.IO.FileStream(FileName, System.IO.FileMode.Open);
            TweetUser user = (TweetUser)bf.Deserialize(fs); 
            fs.Close();
            return user;
        }
    }

}
