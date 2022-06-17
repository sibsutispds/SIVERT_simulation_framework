/*
Copyright (c) <2022> <Aleksei Fedorov, aleksei.fedorov@eit.lth.se; Nikita Lyamin, nikita.lyamin@volvocars.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Data;
using UnityEngine;
using Mono.Data.Sqlite;
using UnityEngine.SceneManagement;


namespace Veneris.Vehicle.Sivert
{
    public class SivertSqLiteLogger : MonoBehaviour
    {

	    public string PassToDB;
        public string dbName = "SIVERTlogDB";
        private string dbAccessName;
        private SqliteConnection SqlConPointer;
        public bool ActivateLoggingToDebug = false;

		private void Awake()
		{
			PassToDB = Application.dataPath;
			PassToDB = PassToDB.Replace("/Assets", "/results/");
			PassToDB += SceneManager.GetActiveScene().name;
			dbName = PassToDB + "/" + dbName;
			dbName += ".db";
			// dbAccessName = "Data Source=" + dbName + ";Version=3;";
			dbAccessName = "Data Source=" + dbName;
			SqliteConnection.CreateFile(dbName);
			SqlConPointer = new SqliteConnection(dbAccessName);
			SqlConPointer.Open();

			CreateSchema();

		}

		public void CreateSchema() {
			using (var cmd = SqlConPointer.CreateCommand()) {
				cmd.CommandType = CommandType.Text;

				var CommandText1 = "CREATE TABLE IF NOT EXISTS 'Messages'( " +
				                   "'VehID' INTEGER," +
				                   "'V2Xstack' TEXT," +
				                   "'UnityTime' REAL," +
				                   "'TxID' INTEGER," +
				                   "'RxID' INTEGER," +
				                   "'MessageBody' TEXT," +
				                   "'RSS' REAL," +
				                   "'Sent' INTEGER," +
				                   "'NS3time' REAL" +
				                   ");";

				var CommandText2 = "CREATE TABLE IF NOT EXISTS 'Kinematics'( " +
				                  "'VehID' INTEGER," +
				                  "'VehName' TEXT NOT NULL," +
				                  "'UnityTime' FLOAT," +
				                  "'PosX' FLOAT," +
				                  "'PosZ' FLOAT," +
				                  "'PosY' FLOAT," +
				                  "'Speed' FLOAT" +
				                  ");";

				var CommandText3 = "CREATE TABLE IF NOT EXISTS 'GSCM'( " +
				                   "'Channel' TEXT," +
				                   "'TxID' INTEGER," +
				                   "'RxID' INTEGER," +
				                   "'UnityTime' FLOAT," +
				                   "'RSS' FLOAT" +
				                   ");";

				cmd.CommandText = CommandText1 + CommandText2 + CommandText3;
				var result = cmd.ExecuteNonQuery();
				if (ActivateLoggingToDebug)
				{
					Debug.Log("create DB schema: " + result);
				}

			}

		}

		public void InserGSCMData(string Channel, int TxID, int RxID, float UnityTime, float RSS)
		{
			using (var cmd = SqlConPointer.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "INSERT INTO GSCM (Channel, TxID, RxID, UnityTime, RSS) " +
				                  "VALUES (@Channel, @TxID, @RxID, @UnityTime, @RSS);";

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "Channel",
					Value = Channel
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "TxID",
					Value = TxID
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "RxID",
					Value = RxID
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "UnityTime",
					Value = UnityTime
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "RSS",
					Value = RSS
				});


				var result = cmd.ExecuteNonQuery();
				if (ActivateLoggingToDebug)
				{
					Debug.Log("insert GSCM: " + result);
				}

			}
		}
		public void InsertKinematicsData(int VehID, string VehName, float UnityTime, float PosX, float PosZ, float PosY, float Speed)
		{
			using (var cmd = SqlConPointer.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "INSERT INTO Kinematics (VehID, VehName, UnityTime, PosX, PosZ, PosY, Speed) " +
				                  "VALUES (@VehID, @VehName, @UnityTime, @PosX, @PosZ, @PosY, @Speed);";

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "VehID",
					Value = VehID
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "VehName",
					Value = VehName
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "UnityTime",
					Value = UnityTime
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "PosX",
					Value = PosX
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "PosZ",
					Value = PosZ
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "PosY",
					Value = PosY
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "Speed",
					Value = Speed
				});


				var result = cmd.ExecuteNonQuery();
				if (ActivateLoggingToDebug)
				{
					Debug.Log("insert Kinematic data: " + result);
				}

			}
		}

		public void InsertMessageData(int VehID, string V2Xstack, float UnityTime, int TxID, int RxID, string MessageBody, float RSS, int Sent, double NS3time)
		{
			using (var cmd = SqlConPointer.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "INSERT INTO Messages (VehID, V2Xstack, UnityTime, TxID, RxID, MessageBody, RSS, Sent, NS3time) " +
				                  "VALUES (@VehID, @V2Xstack, @UnityTime, @TxID, @RxID, @MessageBody, @RSS, @Sent, @NS3time);";

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "VehID",
					Value = VehID
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "V2Xstack",
					Value = V2Xstack
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "UnityTime",
					Value = UnityTime
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "TxID",
					Value = TxID
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "RxID",
					Value = RxID
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "MessageBody",
					Value = MessageBody
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "RSS",
					Value = RSS
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "Sent",
					Value = Sent
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "NS3time",
					Value = NS3time
				});


				var result = cmd.ExecuteNonQuery();
				if (ActivateLoggingToDebug)
				{
					Debug.Log("insert Message data: " + result);
				}
			}
		}



		public void InsertScore(string highScoreName, int score) {

			using (var cmd = SqlConPointer.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "INSERT INTO high_score (name, score) " +
				                  "VALUES (@Name, @Score);";

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "Name",
					Value = highScoreName
				});

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "Score",
					Value = score
				});

				var result = cmd.ExecuteNonQuery();
				Debug.Log("insert score: " + result);
			}

		}

		public void GetHighScores(int limit) {

			using (var cmd = SqlConPointer.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT * FROM high_score ORDER BY score DESC LIMIT @Count;";

				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "Count",
					Value = limit
				});

				Debug.Log("scores (begin)");
				var reader = cmd.ExecuteReader();
				while (reader.Read()) {
					var id = reader.GetInt32(0);
					var highScoreName = reader.GetString(1);
					var score = reader.GetInt32(2);
					var text = string.Format("{0}: {1} [#{2}]", highScoreName, score, id);
					Debug.Log(text);
				}
				Debug.Log("scores (end)");
			}

		}

		private void OnApplicationQuit()
		{
			SqlConPointer.Close();
		}
    }
}
