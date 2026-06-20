namespace ToeicBackend.Application.Enums;

public enum ActivityType
{
    VocabFlashcardReview,
    VocabTyping,
    VocabSpeaking,
    VocabMatching,
    VocabSentence,
    PracticeComplete,
    ExamComplete,
    GrammarLessonComplete,
    GrammarExercise,
    ListeningComplete,
    ReadingComplete,
    SpeakingComplete,
    WritingComplete,
    // Exam types — lifetime anti-spam (mỗi examSetId chỉ cộng 1 lần vĩnh viễn)
    SpeakingExamComplete,
    WritingExamComplete,
}
