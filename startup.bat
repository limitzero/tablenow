:: run the migrations first on the data contexts to seed the application with data
dotnet ef database update --project YourDataProjectName --startup-project YourWebProjectName


:: bring up the back-end components first
start "TableNow - BackEnd" dotnet run --project .server\src\Api\CM.TableNow.Api.csproj


pushd
cd client & ng serve
popd


