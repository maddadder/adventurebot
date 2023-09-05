cd backup
python3 -m venv --system-site-packages ./venv
source ./venv/bin/activate  # sh, bash, or zsh
pip install --upgrade pip
#pip list  # show packages installed within the virtual environment
pip install -r requirements.txt


# To run
source ./venv/bin/activate
python app.py > backup.json
