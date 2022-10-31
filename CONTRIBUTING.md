# Contributing

## Setting up MySQL

Project Lighthouse requires a MySQL database. For Linux users running docker, one can be set up using
the `docker-compose.yml` file in the root of the project folder.

Next, make sure the `LIGHTHOUSE_DB_CONNECTION_STRING` environment variable is set correctly. By default, it
is `server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse`. If you are running the database via the
above `docker-compose.yml` you shouldn't need to change this. For other development/especially production environments
you will need to change this.

Once you've gotten MySQL running you can run Lighthouse. It will take care of the rest.

## Connecting (PS3)

You can use the Remote Patch utility in [UnionPatcher](https://github.com/LBPUnion/UnionPatcher) to manually patch your EBOOT.BIN; it works over the network and automatically downloads, decrypts, patches, encrypts, and uploads your PSN and Disc EBOOTs.

If you are not using a PS3, see [the RPCS3 section](#connecting-rpcs3).

## Connecting (RPCS3)

Start by getting a copy of LittleBigPlanet 1/2/3 installed. (Check the LittleBigPlanet 1 section, since you'll need to do
extra steps for your game to not crash upon entering pod computer). 

The game can be a digital copy (NPUA80472/NPUA80662/NPUA81116) or a disc copy (BCUS98148/BCUS98245/BCUS98362).

Next, download [UnionPatcher](https://github.com/LBPUnion/UnionPatcher/). Binaries can be found by reading the `README.md`
file.

You should have everything you need now, so open up RPCS3 and go to Utilities -> Decrypt PS3 Binaries. Point this
to `rpcs3/dev_hdd0/game/(title id)/USRDIR/EBOOT.BIN`. You can grab your title id by right clicking the game in RPCS3 and
clicking Copy Info -> Copy Serial.


This should give you a file named `EBOOT.elf` in the same folder. This is your decrypted eboot.

Now that you have your decrypted eboot, open UnionPatcher and select the `EBOOT.elf` you got earlier in the top box,
enter `http://localhost:10060/LITTLEBIGPLANETPS3_XML` in the second, and the output filename in the third. For this
guide I'll use `EBOOTlocalhost.elf`.

Now, copy the `EBOOTlocalhost.elf` file to where you got your `EBOOT.elf` file from, and you're now good to go.

To launch the game with the patched EBOOT, open up RPCS3, go to File, Boot SELF/ELF, and open up `EBOOTlocalhost.elf`.

Assuming you patched the file correctly, the database is migrated, and
Project Lighthouse is running, the game should now connect, and you may begin contributing!

### LittleBigPlanet 1 (RPCS3)

For LittleBigPlanet 1 to work with RPCS3, follow the steps above normally.

First, open your favourite hex editor. We recommend [HxD](https://mh-nexus.de/en/hxd/).

Once you have a hex editor open, open your `EBOOTlocalhost.elf` file and search for the hex
values `73 63 65 4E 70 43 6F 6D 6D 65 72 63 65 32`. In HxD, this would be done by clicking on Search -> Replace,
clicking on the `Hex-values` tab, and entering the hex there.

Then, you can zero it out by replacing it with `00 00 00 00 00 00 00 00 00 00 00 00 00 00`.

What this does is remove all the references to the `sceNpCommerce2` function. The function is used for purchasing DLC,
which at this moment in time crashes RPCS3.

After saving the file your LBP1 EBOOT can be used with RPCS3.

## Contributing Tips

### Database migrations

Some modifications may require updates to the database schema. You can automatically create a migration file by:

1. Making sure the tools are installed. You can do this by running `dotnet tool restore`.
2. Making sure `LIGHTHOUSE_DB_CONNECTION_STRING` is set correctly. See the `Running` section for more details.
3. Modifying the database schema via the C# portion of the code. Do not modify the actual SQL database.
4. Running `dotnet ef migrations add <NameOfMigrationInPascalCase> --project ProjectLighthouse`.

This process will create a migration file from the changes made in the C# code.

The new migrations will automatically be applied upon starting Lighthouse.

### Running tests

You can run tests either through your IDE or by running `dotnet tests`.

Keep in mind while running database tests (which most tests are) you need to have `LIGHTHOUSE_DB_CONNECTION_STRING` set.

### Continuous Integration (CI) Tips

- You can skip CI runs for a commit if you specify `[skip ci]` at the beginning of the commit name. This is useful for
  formatting changes, etc.
- When creating your first pull request, CI will not run initially. A team member will have to approve you for use of
  running CI on a pull request. This is because of GitHub policy.

### API Documentation

You can access API documentation by looking at the XMLDoc in the controllers under `ProjectLighthouse.Controllers.Api`

You can also access an interactive version by starting Lighthouse and accessing Swagger
at `http://localhost:10060/swagger/index.html`.
