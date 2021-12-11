# Project Lighthouse

Project Lighthouse is an umbrella project for all work to investigate and develop private servers for LittleBigPlanet.
This project is the main server component that LittleBigPlanet games connect to.

## WARNING!

This is beta software, and thus is not ready for public use yet. We're not responsible if someone connects and hacks
your entire machine and deletes all your files.

That said, feel free to develop privately!

## Building

This will be written when we're out of beta. Consider this your barrier to entry ;).

## Running

Lighthouse requires a MySQL database at this time. For Linux users running docker, one can be set up using
the `docker-compose.yml` file in the root of the project folder.

Next, make sure the `LIGHTHOUSE_DB_CONNECTION_STRING` environment variable is set correctly. By default, it
is `server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse`. If you are running the database via the
above `docker-compose.yml` you shouldn't need to change this. For other development/especially production environments
you will need to change this.

Once you've gotten MySQL running you can run Lighthouse. It will take care of the rest.

## Connecting

PS3 is difficult to set up, so I will be going over how to set up RPCS3 instead. A guide will be coming for PS3 closer
to release. You can also follow this guide if you want to learn how to modify your EBOOT.

*Note: This requires a modified copy of RPCS3. You can find a working
patch [here](https://gist.github.com/jvyden/0d9619f7dd3dbc49f7583486bdacad75).*

Start by getting a copy of LittleBigPlanet 2 installed. It can be digital (NPUA80662) or disc (BCUS98245). I won't get
into how because if you got this far you should already know what you're doing. For those that don't,
the [RPCS3 Quickstart Guide](https://rpcs3.net/quickstart) should cover it.

Next, download [UnionPatcher](https://github.com/LBPUnion/UnionPatcher/). Binaries can be found by reading the README.md
file.

You should have everything you need now, so open up RPCS3 and go to Utilities -> Decrypt PS3 Binaries. Point this
to `rpcs3/dev_hdd0/game/(title id)/USRDIR/EBOOT.BIN`.

This should give you a file named `EBOOT.elf` in the same folder. Next, fire up UnionPatcher (making sure to select the
correct project to start, e.g. on Mac launch `UnionPatcher.Gui.MacOS`.)

Now that you have your decrypted eboot, open UnionPatcher and select the `EBOOT.elf` you got earlier in the top box,
enter `http://localhost:10060/LITTLEBIGPLANETPS3_XML` in the second, and the output filename in the third. For this
guide I'll use `EBOOTlocalhost.elf`.

Now, copy the `EBOOTlocalhost.elf` file to where you got your `EBOOT.elf` file from, and you're now good to go.

To launch the game with the patched EBOOT, open up RPCS3, go to File, Boot SELF/ELF, and open up `EBOOTlocalhost.elf`.

Assuming you are running the patched version of RPCS3, you patched the file correctly, the database is migrated, and
Lighthouse is running, the game should now connect.

Finally, take a break. Chances are that took a while.

## Contributing Tips

### Database migrations

Some modifications may require updates to the database schema. You can automatically create a migration file by:

1. Making sure the tools are installed. You can do this by running `dotnet tool restore`.
2. Making sure `LIGHTHOUSE_DB_CONNECTION_STRING` is set correctly. See the `Running` section for more details.
3. Making your changes to the database. I won't cover this since if you're making database changes you should know what
   you're doing.
4. Running `dotnet ef migrations add <NameOfMigrationInPascalCase> --project ProjectLighthouse`.

### Running tests

You can run tests either through your IDE or by running `dotnet tests`.

Keep in mind while running database tests you need to have `LIGHTHOUSE_DB_CONNECTION_STRING` set.

## Compatibility across games and platforms

| Game     | Console (PS3/Vita/PSP)                | Emulator (RPCS3/Vita3k/PPSSPP)                           | Next-Gen (PS4/PS5)     |
|----------|---------------------------------------|----------------------------------------------------------|------------------------|
| LBP1     | Compatible                            | Incompatible, crashes on entering pod computer           | N/A                    |
| LBP2     | Compatible                            | Compatible with patched RPCS3                            | N/A                    |
| LBP3     | Somewhat compatible, frequent crashes | Somewhat compatible with patched RPCS3, frequent crashes | Incompatible           |
| LBP Vita | Compatible                            | Incompatible, marked as "bootable" on Vita3k             | N/A                    |
| LBP PSP  | Potentially compatible                | Incompatible, PSN not supported on PPSSPP                | Potentially Compatible |

While LBP Vita and LBP PSP can be supported, they are not properly seperated from the mainline games at this time. We
recommend you run seperate instances for these games to avoid problems.

Project Lighthouse is still a heavy work in progress, so this chart is subject to change at any point.
