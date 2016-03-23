if [ ! -f ".git/hooks/commit-msg" ]; then
    echo "Creating commit-msg hook"
    cp githooks/commit-msg .git/hooks
    chmod +x .git/hooks/commit-msg
else
    echo "commit-msg hook already exists"
fi
