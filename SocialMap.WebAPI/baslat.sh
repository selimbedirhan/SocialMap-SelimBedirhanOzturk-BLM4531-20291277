npx kill-port 5280 5173 && npx concurrently -n "API,UI" -c "blue,green" "cd SocialMap.WebAPI && dotnet run" "cd frontend && npm run dev"
