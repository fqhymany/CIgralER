function NowDate(props) {
  const currentDate = new Date();

  const options = {weekday: 'long', month: 'long', day: 'numeric'};
  const formattedDate = currentDate.toLocaleDateString('fa-IR', options);
  return <div className={`fs-7 ${props.display}`}>امروز : {formattedDate}</div>;
}
export default NowDate;
