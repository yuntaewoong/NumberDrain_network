public class SubmittedInfo//masterclient에서 처리하는 승부에 관한 정보
{
    public SubmittedInfo(Card card, int actorNum)
    {
        this.card = card;
        this.actorNum = actorNum;
    }
    public Card GetCard()
    {
        return card;
    }
    public int GetActorNum()
    {
        return actorNum;
    }
    private Card card;
    private int actorNum;
}