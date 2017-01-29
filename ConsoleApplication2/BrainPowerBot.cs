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
                    if (!Connected)
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
                await bot.Connect(TOKEN, TokenType.Bot);
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
    }
    /*
    public static void PlayMp3FromUrl(string url)
    {
        using (Stream ms = new MemoryStream())
        {
            using (Stream stream = WebRequest.Create(url)
                .GetResponse().GetResponseStream())
            {
                byte[] buffer = new byte[32768];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
            }

            ms.Position = 0;
            using (WaveStream blockAlignedStream =
                new BlockAlignReductionStream(
                    WaveFormatConversionStream.CreatePcmStream(
                        new Mp3FileReader(ms))))
            {
                using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                {
                    waveOut.Init(blockAlignedStream);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }
    }
    */
}
