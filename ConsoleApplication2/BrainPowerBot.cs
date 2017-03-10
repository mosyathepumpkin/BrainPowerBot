/*
TOKEN хранит токен (логично)
ADMIN хранит никнейм того, кто сможет полностью использовать все фичи бота, т.е. дополнительные команды:
!clear (удаляет сообщения, нужны права),
!block (блокирует управление аудиопотоком для использования только админом)
*/

using Discord;
using Discord.Audio;
using Discord.Commands;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using VideoLibrary;
/*
using System.IO;
using System.Net;
*/

class Program
{
    static void Main(string[] args) => new Program().Start();
    //константы
    private string TOKEN = "MjcxMDkyNDY3MDMyMzkxNjgw.C2BaRQ.p8o_QMMOt5lajW_BT2i6PtDnbr4";
    private string ADMIN = "FolMing";
    //переменные
    private DiscordClient bot;
    private IAudioClient _vClient;
    private bool Connected = false;
    private bool _isStopped = true;
    private bool _isPaused = false;
    private bool _isBlocked = false;

    public void Start()
    {
        bot = new DiscordClient();

        bot.UsingCommands(x =>
        {
            x.PrefixChar = '!';
            x.AllowMentionPrefix = false;
        });

        CreateCommands();
        //Управление аудиопотоком
        bot.MessageReceived += async (s, e) =>
        {
            if (_isBlocked == true && e.User.Name == ADMIN && e.Channel.IsPrivate == false || _isBlocked == false && e.Channel.IsPrivate == false)
            {
                if (e.Message.Text == "!play" && !Connected)
                {
                    var VoiceChannel = e.User.VoiceChannel;
                    if (!(VoiceChannel == null))
                    {
                        _isStopped = false;
                        _isPaused = false;
                        Connected = true;
                        _vClient = await bot.GetService<AudioService>().Join(VoiceChannel);
                        SendAudio("C:\\music.mp3");
                    }
                }
                if (e.Message.Text == "!stop" && Connected) // music control
                {
                    _isStopped = true;
                    _isPaused = false;
                }
                if (e.Message.Text == "!pause" && Connected)
                {
                    _isStopped = false;
                    _isPaused = true;
                }
                if (e.Message.Text == "!play" && Connected)
                {
                    _isStopped = false;
                    _isPaused = false;
                }
            }
        };
        bot.UsingAudio(x =>
        {
            x.Mode = AudioMode.Outgoing;
        });
        bot.ExecuteAndWait(async () => // Connection
        {
            try
            {
                //await bot.Connect(TOKEN, TokenType.Bot);
                await bot.Connect("6870920@mail.ru", "qwerty123456");
                bot.SetGame(new Game("sexual games with your mother"));
                System.Console.WriteLine("Logged in as " + bot.CurrentUser.Name);
            }
            catch (Discord.Net.HttpException)
            {
                System.Console.WriteLine("Can't login.");
                System.Console.ReadLine();
                System.Environment.Exit(1);
            }
        });
    }

    //здесь все команды за исключением управления аудио (потому что я ленивый)
    public void CreateCommands()
    {
        var cService = bot.GetService<CommandService>();
        //--------------------------------------------------------------------------------------------
        cService.CreateCommand("clear")
            .Do(async (e) =>
            {
                if (e.User.Name == ADMIN)
                {
                    int nummessages = 0;
                    var inumerator = e.Channel.Messages.GetEnumerator();
                    do
                    {
                        inumerator.MoveNext();
                        nummessages++;
                    } while (inumerator.Current != null);
                    Message[] messagesToDelete;
                    messagesToDelete = await e.Channel.DownloadMessages();

                    await e.Channel.DeleteMessages(messagesToDelete);
                }
            });
        //--------------------------------------------------------------------------------------------
        cService.CreateCommand("matyxa")
            .Do(async (e) =>
            {
                if (e.Channel.IsPrivate == false)
                {
                    var users = e.Channel.Users;
                    var usersnum = e.Server.UserCount;
                    User user;
                    do
                    {
                        Random rand = new Random();
                        int randomnum = rand.Next(0, usersnum);
                        var iterator = users.GetEnumerator();
                        int i = -1;
                        do
                        {
                            iterator.MoveNext();
                            i++;
                        } while (i < randomnum);
                        user = iterator.Current;
                    } while (user.Name == bot.CurrentUser.Name);
                    await e.Channel.SendMessage(user.Mention + " matyxy 4ekni");
                }
            });
        //--------------------------------------------------------------------------------------------
        cService.CreateCommand("block")
            .Do(async (e) =>
            {
                if (e.User.Name == ADMIN)
                {
                    switch (_isBlocked)
                    {
                        case false: _isBlocked = true; await e.Channel.SendMessage(e.User.Mention + " Bot is now blocked"); break;
                        case true: _isBlocked = false; await e.Channel.SendMessage(e.User.Mention + " Bot is now not blocked"); break;
                    }
                }
            });
        //--------------------------------------------------------------------------------------------
        cService.CreateCommand("restrict")
            .Do(async (e) =>
            {
                DateTime daterestrict = new DateTime(2017, 1, 3, 3, 50, 0);
                DateTime datenow = DateTime.Now;
                TimeSpan datesub = datenow.Subtract(daterestrict);
                int days = datesub.Days;
                int minutes = datesub.Minutes;
                int seconds = datesub.Seconds;
                var users = e.Server.FindUsers("Soosiezi");
                var iterator = users.GetEnumerator();
                iterator.MoveNext();
                if (iterator.Current == null) { }
                else
                {
                    await e.Channel.SendMessage(iterator.Current.Mention + " уже в рестрикте " + days + " дней, " + minutes + " минут и " + seconds + " секунд :)");
                }
            });
        cService.CreateCommand("bantime")
            .Do(async (e) =>
            {
                if (e.User.Name == "FolMing")
                {
                    DateTime daterestrict = new DateTime(2017, 5, 30, 0, 10, 0);
                    DateTime datenow = DateTime.Now;
                    TimeSpan datesub = daterestrict.Subtract(datenow);
                    await e.Channel.SendMessage(e.User.Mention + " в рестрикте осталось сидеть " + datesub.Days + " дней, " + datesub.Minutes + " минут и " + datesub.Seconds + " секунд :)");
                }
            });
        cService.CreateCommand("findusers")
            .Parameter("search", ParameterType.Required)
            .Do(async (e) =>
            {
                var users = e.Server.FindUsers(e.GetArg("search"));
                var iterator = users.GetEnumerator();
                do
                {
                    iterator.MoveNext();
                    await e.Channel.SendMessage(iterator.Current.Mention);
                } while (iterator.Current != null);
            });
        cService.CreateCommand("ботлох")
            .Do(async (e) =>
            {
                await e.Channel.SendMessage("сам лох, я работаю на машине с 6 ГГц на воздухе");
                await e.Channel.SendFile("C:\\unnamed.jpg");
            });
        cService.CreateCommand("playURL")
            .Parameter("URL", ParameterType.Required)
            .Do(async (e) =>
            {
                if (!Connected)
                {
                    var VoiceChannel = e.User.VoiceChannel;
                    if (!(VoiceChannel == null))
                    {
                        _isStopped = false;
                        _isPaused = false;
                        Connected = true;
                        _vClient = await bot.GetService<AudioService>().Join(VoiceChannel);
                        PlayMp3FromUrl(e.GetArg("URL"));
                    }
                }
            });
        cService.CreateCommand("playYT")
            .Parameter("URL", ParameterType.Required)
            .Do(async (e) =>
            {
                if (!Connected)
                {
                    var VoiceChannel = e.User.VoiceChannel;
                    if (!(VoiceChannel == null))
                    {
                        _isStopped = false;
                        _isPaused = false;
                        Connected = true;
                        _vClient = await bot.GetService<AudioService>().Join(VoiceChannel);
                        PlayFromYoutube(e.GetArg("URL"));
                    }
                }
            });
        cService.CreateCommand("acc")
            .Do(async (e) =>
            {
                await e.Channel.SendFile("C:\\acc.jpg");
            });
        cService.CreateCommand("transform")
            .Parameter("p", ParameterType.Required)
            .Do(async (e) =>
            {
                bool fl = false;
                var text1 = e.GetArg("p");
                string text2 = null;
                for (int i = 0; i < text1.Length; i++)
                {
                    switch (text1[i])
                    {
                        case 'A':
                        case 'a': text2 += ":regional_indicator_a:"; break;
                        case 'B':
                        case 'b': text2 += ":regional_indicator_b:"; break;
                        case 'C':
                        case 'c': text2 += ":regional_indicator_c:"; break;
                        case 'D':
                        case 'd': text2 += ":regional_indicator_d:"; break;
                        case 'E':
                        case 'e': text2 += ":regional_indicator_e:"; break;
                        case 'F':
                        case 'f': text2 += ":regional_indicator_f:"; break;
                        case 'G':
                        case 'g': text2 += ":regional_indicator_g:"; break;
                        case 'H':
                        case 'h': text2 += ":regional_indicator_h:"; break;
                        case 'I':
                        case 'i': text2 += ":regional_indicator_i:"; break;
                        case 'J':
                        case 'j': text2 += ":regional_indicator_j:"; break;
                        case 'K':
                        case 'k': text2 += ":regional_indicator_k:"; break;
                        case 'L':
                        case 'l': text2 += ":regional_indicator_l:"; break;
                        case 'M':
                        case 'm': text2 += ":regional_indicator_m:"; break;
                        case 'N':
                        case 'n': text2 += ":regional_indicator_n:"; break;
                        case 'O':
                        case 'o': text2 += ":regional_indicator_o:"; break;
                        case 'P':
                        case 'p': text2 += ":regional_indicator_p:"; break;
                        case 'Q':
                        case 'q': text2 += ":regional_indicator_q:"; break;
                        case 'R':
                        case 'r': text2 += ":regional_indicator_r:"; break;
                        case 'S':
                        case 's': text2 += ":regional_indicator_s:"; break;
                        case 'T':
                        case 't': text2 += ":regional_indicator_t:"; break;
                        case 'U':
                        case 'u': text2 += ":regional_indicator_u:"; break;
                        case 'V':
                        case 'v': text2 += ":regional_indicator_v:"; break;
                        case 'W':
                        case 'w': text2 += ":regional_indicator_w:"; break;
                        case 'X':
                        case 'x': text2 += ":regional_indicator_x:"; break;
                        case 'Y':
                        case 'y': text2 += ":regional_indicator_y:"; break;
                        case 'Z':
                        case 'z': text2 += ":regional_indicator_z:"; break;
                        case '1': text2 += ":one:"; break;
                        case '2': text2 += ":two:"; break;
                        case '3': text2 += ":three:"; break;
                        case '4': text2 += ":four:"; break;
                        case '5': text2 += ":five:"; break;
                        case '6': text2 += ":six:"; break;
                        case '7': text2 += ":seven:"; break;
                        case '8': text2 += ":eight:"; break;
                        case '9': text2 += ":nine:"; break;
                        case '0': text2 += ":zero:"; break;
                        case ' ': text2 += "    "; break;
                        default: fl = true; break;
                    }
                }
                if (fl == false) await e.Channel.SendMessage(text2);
            });
    }
    public void SendAudio(string filePath) // Audio sending control
    {
        var channelCount = bot.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
        var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
        using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
        using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
        {
            resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
            int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
            byte[] buffer = new byte[blockSize];
            int byteCount;
            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
            {
                if (byteCount < blockSize)
                {
                    // Incomplete Frame
                    for (int i = byteCount; i < blockSize; i++)
                        buffer[i] = 0;
                }
                _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                if (_isPaused) do { } while (_isPaused);
                if (_isStopped) break;
            }
            _vClient.Disconnect();
            _isStopped = true;
            _isPaused = false;
            Connected = false;
        }
    }
    public void PlayMp3FromUrl(string url)
    {
        using (var client = new WebClient())
        {
            client.DownloadFile(url, "D:\\temp.mp3");
            SendAudio("D:\\temp.mp3");
        }
    }
    public void PlayFromYoutube(string URL)
    {
        var youTube = YouTube.Default;
        var videos = youTube.GetAllVideos(URL);
        var iterator = videos.GetEnumerator();
        var length = videos.Count();
        int maxbitrate = 0;
        int number = 0;
        for (int i = 0; i < length; i++)
        {
            iterator.MoveNext();
            if (iterator.Current.AudioBitrate > maxbitrate && iterator.Current.FileExtension.Equals(".mp4"))
            {
                maxbitrate = iterator.Current.AudioBitrate;
                number = i;
            }

        }
        iterator = videos.GetEnumerator();
        for (int i = 0; i < number + 1; i++) iterator.MoveNext();
        if (iterator.Current.IsEncrypted == false)
        {
            File.WriteAllBytes("D:\\video.mp4", iterator.Current.GetBytes());
            string arg = @"-i D:\video.mp4 -y -vn -f mp3 -ab 192k D:\temp.mp3";
            string name = @"C:\ffmpeg\bin\ffmpeg.exe";
            Process process = new Process();
            process.StartInfo.FileName = name;
            process.StartInfo.Arguments = arg;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            do { } while (process.HasExited == false);
            SendAudio("D:\\temp.mp3");
        }
        else
        { 
            _isStopped = true;
            _isPaused = false;
            Connected = false;
            _vClient.Disconnect();
        }
    }
}
